using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using esdc_sa_appdev_reporting_api.Models;
using esdc_sa_appdev_reporting_api.Constants;
using Newtonsoft.Json;

namespace esdc_sa_appdev_reporting_api.Services
{
    public class TrelloService
    {
        public async Task<SummaryReportData> GetSummaryReportResults()
        {
            /*
                The resulting result matrix must have the following format:
                    [
						[' ','Backlog','Committed','In Progress','Blocked / On Hold','Done'],
                        ['BDM', '6', '12', '5', '2', '6'],
                        ['OAS-SIS', '7', '4', '1', '1', '3'],
                        ['DTS', '1', '1', '0', '3', '1'],
                        ['CCOE', '2', '6', '3', '2', '1']
                    ]
                
                where the first row contains a non-empty string as the first item and the client titles
                then, where the first item of each result set is the client and the remaining are 
                the tallied number of cards per list in order of Backlog to Done.        
            */

            var compiledResults = new List<List<string>>();

            var reportResultItems = await this.GetGeneralReportResults();

            // Extract the results that have a Client label
            var applicableResults = reportResultItems
                .Where(x => x.CardLabels.Where(y => y.Color.Equals(SolutionConstants.TrelloLabelCategory.Client)).Any())
                .ToList();

            // Compile the count of cards per list by sub-category, where sub-category equals color "Client"
            // The first-level key is the Client label, the second-level is the Trello list
            var compiledResultMatrix = new Dictionary<string, Dictionary<string, int>>();

            foreach (var result in applicableResults)
            {
                var clientKey = this.ExtractClientName(result.CardLabels);

                // Tally the nbr of cards
                if (compiledResultMatrix.ContainsKey(clientKey) == false)
                {
                    compiledResultMatrix.Add(clientKey, new Dictionary<string, int>(){{result.CardCurrentListTitle, 1}});
                }
                else
                {
                    if (compiledResultMatrix[clientKey].ContainsKey(result.CardCurrentListTitle) == false)
                    {
                        compiledResultMatrix[clientKey].Add(result.CardCurrentListTitle, 1);
                    }
                    else
                    {
                        compiledResultMatrix[clientKey][result.CardCurrentListTitle] ++;
                    }
                }
            }

            // For each client, fetch the total for each Trello list
            foreach (var client in compiledResultMatrix)
            {
                var trelloClientValues = new List<string>();

                // Add the first value: client name
                trelloClientValues.Add(client.Key);

                // Add the nbr of cards value in explicit list order
                foreach (var trelloList in SolutionConstants.TrelloListIndexMap.Map)
                {
                    // Check to see if there's a compiled value for the intended client + trello list, otherwise assign 0.
                    if (compiledResultMatrix[client.Key].ContainsKey(trelloList.Value) == true)
                    {
                        trelloClientValues.Add(compiledResultMatrix[client.Key][trelloList.Value].ToString());
                    }
                    else
                    {
                        trelloClientValues.Add("0");
                    }
                }

                compiledResults.Add(trelloClientValues);
            }

            // Order the clients alphabetically
            compiledResults = compiledResults.OrderBy(x => x.FirstOrDefault()).ToList();
            
            // Add the first result row.
            var firstResultSetValues = new List<string>();

            firstResultSetValues.Add(" ");
            firstResultSetValues.Add(SolutionConstants.TrelloLists.Backlog);
            firstResultSetValues.Add(SolutionConstants.TrelloLists.Committed);
            firstResultSetValues.Add(SolutionConstants.TrelloLists.InProgress);
            firstResultSetValues.Add(SolutionConstants.TrelloLists.OnHold);
            firstResultSetValues.Add(SolutionConstants.TrelloLists.Done);

            compiledResults.Insert(0, firstResultSetValues);

            // Gather raw results
            var reportItemResults = new List<SummaryReportItemResult>();

            foreach(var card in applicableResults)
            {
                var reportItemResult = new SummaryReportItemResult();

                reportItemResult.TaskName = card.CardTitle;
                reportItemResult.ClientName = this.ExtractClientName(card.CardLabels);
                reportItemResult.StatusTitle = card.CardCurrentListTitle;
                reportItemResult.DateStarted = card.CardDateStarted?.ToString(CoreConstants.Formats.DateIso);
                reportItemResult.DateCompleted = card.CardDateCompleted?.ToString(CoreConstants.Formats.DateIso);
                reportItemResult.AssignedTo = string.Join(", ", card.CardMembers.Select(x => x.Value).ToList());
                reportItemResult.Url = card.CardUrl;

                reportItemResults.Add(reportItemResult);
            }

            // Assign result model values
            var resultModel = new SummaryReportData();

            resultModel.CompiledResults = compiledResults;
            resultModel.ReportItemResults = reportItemResults;

            return resultModel;
        }


        public async Task<List<GeneralReportResult>> GetGeneralReportResults()
        {
            // 1. Gather all cards
            // 2. Compute date started and completed
            //    2.a) Cards that were transferred from Backlog or Committed
            //    2.b) Cards that were created directly in either In Progress, On Hold or Done.
            // 3. Compute number of days on hold

            var results = new List<GeneralReportResult>();

            var trelloLists = await this.GetTrelloLists();
            var trelloLabels = await this.GetTrelloLabels();
            var trelloMembers = await this.GetTrelloMembers();
            var trelloCards = await this.GetAllTrelloCards();
            var trelloCardMoveActions = await this.GetAllTrelloCardMoveActions();
            var trelloCardCreatedActions = await this.GetAllTrelloCardCreateActions();

            // Pre-sort
            trelloCardMoveActions
                .OrderBy(x => x.data.card)
                .ThenBy(x => x.date);

            // Step 1
            foreach (var card in trelloCards)
            {
                var result = new GeneralReportResult();

                result.CardId = card.id;
                result.CardTitle = card.name;
                result.CardCurrentListId = card.idList;
                result.CardCurrentListTitle = trelloLists.SingleOrDefault(x => x.id == card.idList)?.name;
                result.CardUrl = card.url;
                
                foreach (var labelId in card.idLabels)
                {
                    var trelloLabel = trelloLabels.SingleOrDefault(x => x.id == labelId);

                    if (trelloLabel != null)
                    {
                        var cardLabel = new LabelResult();

                        cardLabel.Id = trelloLabel.id;
                        cardLabel.Name = trelloLabel.name;                    
                        cardLabel.Color = trelloLabel.color;

                        result.CardLabels.Add(cardLabel);
                    }
                }
                                
                foreach (var memberId in card.idMembers)
                {
                    var trelloMember = trelloLabels.SingleOrDefault(x => x.id == memberId);

                    result.CardMembers.Add
                    (
                        trelloMembers.SingleOrDefault(x => x.id == memberId)?.id,
                        trelloMembers.SingleOrDefault(x => x.id == memberId)?.fullName
                    );
                }

                // Card in backlog and committed aren't considered started.
                if ((result.CardCurrentListTitle != SolutionConstants.TrelloLists.Backlog) &&
                    (result.CardCurrentListTitle != SolutionConstants.TrelloLists.Committed))
                {
                    // 2.a)
                    var action = trelloCardMoveActions
                        .Where
                        (   
                            x => 
                                (x.data.card.id == card.id) && 
                                (x.data.listAfter.name == SolutionConstants.TrelloLists.InProgress)
                        )
                        .OrderBy(x => x.date)
                        .FirstOrDefault();

                    // 2.b)
                    if (action == null)
                    {
                        action = trelloCardCreatedActions
                            .Where(x => x.data.card.id == card.id)
                            .OrderBy(x => x.date)
                            .FirstOrDefault();
                    }

                    result.CardDateStarted = action?.date;
                }

                // Only cards in done are considered completed.
                if (result.CardCurrentListTitle == SolutionConstants.TrelloLists.Done)
                {
                    var action = trelloCardMoveActions
                        .Where
                        (
                            x => 
                                (x.data.card.id == card.id) && 
                                (x.data.listAfter.name == SolutionConstants.TrelloLists.Done)
                        )
                        .OrderByDescending(x => x.date)
                        .FirstOrDefault();

                    result.CardDateCompleted = action?.date;
                }

                results.Add(result);
            }
            
            return results;
        }


        public async Task<List<TrelloListDto>> GetTrelloLists()
        {
            using (var http = new HttpClient())
            {
                try
                {
                    var url = "https://api.trello.com/1/boards/" + SolutionConstants.kTrelloBoardId + "/lists/"
                        + "?key=" + SolutionConstants.kTrelloAppKey 
                        + "&token=" + SolutionConstants.kTrelloUserToken;

                    var response = await http.GetAsync(url);

                    response.EnsureSuccessStatusCode();

                    var jsonResult = await response.Content.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<List<TrelloListDto>>(jsonResult);
                }
                catch (HttpRequestException httpRequestException)
                {
                    Console.WriteLine($"Error in GetTrelloLists: {httpRequestException.Message}");
                }

                return new List<TrelloListDto>();
            }
        }


        public async Task<List<TrelloLabelsDto>> GetTrelloLabels()
        {
            using (var http = new HttpClient())
            {
                try
                {
                    var url = "https://api.trello.com/1/boards/" + SolutionConstants.kTrelloBoardId + "/labels/"
                        + "?key=" + SolutionConstants.kTrelloAppKey 
                        + "&token=" + SolutionConstants.kTrelloUserToken
                        + "&limit=1000";

                    var response = await http.GetAsync(url);

                    response.EnsureSuccessStatusCode();

                    var jsonResult = await response.Content.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<List<TrelloLabelsDto>>(jsonResult);
                }
                catch (HttpRequestException httpRequestException)
                {
                    Console.WriteLine($"Error in GetTrelloLabels: {httpRequestException.Message}");
                }

                return new List<TrelloLabelsDto>();
            }
        }


        public async Task<List<TrelloMemberDto>> GetTrelloMembers()
        {
            using (var http = new HttpClient())
            {
                try
                {
                    var url = "https://api.trello.com/1/boards/" + SolutionConstants.kTrelloBoardId + "/members/"
                        + "?key=" + SolutionConstants.kTrelloAppKey 
                        + "&token=" + SolutionConstants.kTrelloUserToken;

                    var response = await http.GetAsync(url);

                    response.EnsureSuccessStatusCode();

                    var jsonResult = await response.Content.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<List<TrelloMemberDto>>(jsonResult);
                }
                catch (HttpRequestException httpRequestException)
                {
                    Console.WriteLine($"Error in GetTrelloMembers: {httpRequestException.Message}");
                }

                return new List<TrelloMemberDto>();
            }
        }


        public async Task<List<TrelloCardDto>> GetAllTrelloCards()
        {
            var trelloCards = new List<TrelloCardDto>();
            var beforeDate = DateTime.Now;
            var isBreak = false;

            while (isBreak == false)
            {
                var results = await this.GetTrelloCards(beforeDate);

                if (results.Count < SolutionConstants.kTrelloLimitMax)
                {
                    isBreak = true;
                }
                else
                {
                    // Grab the earliest date of the results.
                    var dateMin = results.Min(x => x.DateFromId);

                    // Because the earliest timestamp record may not necessarily be the last record within a given day, the target date is the last date + 1.                    
                    beforeDate = dateMin.AddDays(1).Date;

                    // Only keep the results of the last known full day.
                    results = results.Where(x => x.DateFromId >= beforeDate).ToList();
                }

                trelloCards.AddRange(results);
            }

            return trelloCards;
        }


        public async Task<List<TrelloCardDto>> GetTrelloCards(DateTime beforeDate)
        {
            using (var http = new HttpClient())
            {
                try
                {
                    var url = "https://api.trello.com/1/boards/" + SolutionConstants.kTrelloBoardId + "/cards/"
                        + "?key=" + SolutionConstants.kTrelloAppKey 
                        + "&token=" + SolutionConstants.kTrelloUserToken
                        + "&before=" + beforeDate.ToString(CoreConstants.Formats.DtmZuluIso)
                        + "&limit=" + SolutionConstants.kTrelloLimitMax.ToString();

                    var response = await http.GetAsync(url);

                    response.EnsureSuccessStatusCode();

                    var jsonResult = await response.Content.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<List<TrelloCardDto>>(jsonResult);
                }
                catch (HttpRequestException httpRequestException)
                {
                    Console.WriteLine($"Error in GetTrelloCards: {httpRequestException.Message}");
                }

                return new List<TrelloCardDto>();
            }
        }


        public async Task<List<TrelloActionDto>> GetAllTrelloCardMoveActions()
        {
            var trelloActions = new List<TrelloActionDto>();
            var beforeDate = DateTime.Now;
            var isBreak = false;

            while (isBreak == false)
            {
                var results = await this.GetTrelloCardMoveActions(beforeDate);

                if (results.Count < SolutionConstants.kTrelloLimitMax)
                {
                    isBreak = true;
                }
                else
                {
                    // Grab the earliest date of the results.
                    var dateMin = results.Min(x => x.DateFromId);

                    // Because the earliest timestamp record may not necessarily be the last record within a given day, the target date is the last date + 1.                    
                    beforeDate = dateMin.AddDays(1).Date;

                    // Only keep the results of the last known full day.
                    results = results.Where(x => x.DateFromId >= beforeDate).ToList();
                }

                trelloActions.AddRange(results);
            }

            // Only keep the records that where list transfers.
            return trelloActions.Where(x => (x.IsListTransfer() == true)).ToList();
        }


        public async Task<List<TrelloActionDto>> GetTrelloCardMoveActions(DateTime beforeDate)
        {
            using (var http = new HttpClient())
            {
                try
                {
                    var url = "https://api.trello.com/1/boards/" + SolutionConstants.kTrelloBoardId + "/actions/"
                        + "?key=" + SolutionConstants.kTrelloAppKey 
                        + "&token=" + SolutionConstants.kTrelloUserToken
                        + "&filter=updateCard"
                        //+ "&since=2019-06-01"
                        + "&before=" + beforeDate.ToString(CoreConstants.Formats.DtmZuluIso)
                        + "&limit=" + SolutionConstants.kTrelloLimitMax.ToString();

                    var response = await http.GetAsync(url);

                    response.EnsureSuccessStatusCode();

                    var jsonResult = await response.Content.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<List<TrelloActionDto>>(jsonResult);
                }
                catch (HttpRequestException httpRequestException)
                {
                    Console.WriteLine($"Error in GetTrelloCardMoveActions: {httpRequestException.Message}");
                }

                return new List<TrelloActionDto>();
            }
        }


        public async Task<List<TrelloActionDto>> GetAllTrelloCardCreateActions()
        {
            var trelloActions = new List<TrelloActionDto>();
            var beforeDate = DateTime.Now;
            var isBreak = false;

            while (isBreak == false)
            {
                var results = await this.GetTrelloCardCreateActions(beforeDate);

                if (results.Count < SolutionConstants.kTrelloLimitMax)
                {
                    isBreak = true;
                }
                else
                {
                    // Grab the earliest date of the results.
                    var dateMin = results.Min(x => x.DateFromId);

                    // Because the earliest timestamp record may not necessarily be the last record within a given day, the target date is the last date + 1.                    
                    beforeDate = dateMin.AddDays(1).Date;

                    // Only keep the results of the last known full day.
                    results = results.Where(x => x.DateFromId >= beforeDate).ToList();
                }

                trelloActions.AddRange(results);
            }

            return trelloActions;
        }


        public async Task<List<TrelloActionDto>> GetTrelloCardCreateActions(DateTime beforeDate)
        {
            // Reference: https://stackoverflow.com/questions/51777063/how-can-i-get-all-actions-for-a-board-using-trellos-rest-api

            using (var http = new HttpClient())
            {
                try
                {
                    var url = "https://api.trello.com/1/boards/" + SolutionConstants.kTrelloBoardId + "/actions/"
                        + "?key=" + SolutionConstants.kTrelloAppKey 
                        + "&token=" + SolutionConstants.kTrelloUserToken
                        + "&filter=createCard"
                        + "&before=" + beforeDate.ToString(CoreConstants.Formats.DtmZuluIso)
                        + "&limit=" + SolutionConstants.kTrelloLimitMax.ToString();

                    var response = await http.GetAsync(url);

                    response.EnsureSuccessStatusCode();

                    var jsonResult = await response.Content.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<List<TrelloActionDto>>(jsonResult);
                }
                catch (HttpRequestException httpRequestException)
                {
                    Console.WriteLine($"Error in GetTrelloCardMoveActions: {httpRequestException.Message}");
                }

                return new List<TrelloActionDto>();
            }
        }
        
        #region --- Private -------------------------------------------------------

        private string ExtractClientName (List<LabelResult> cardLabels)
        {
            // Get the first green label of the card (should only be only, but just in case, we'll grab the first).
            var clientLabel = cardLabels.FirstOrDefault(x => x.Color == SolutionConstants.TrelloLabelCategory.Client);

            // Get the label sub-category (meaning, remove the prefix, i.e. "Client: ")
            var posLabelSeparator = clientLabel.Name.IndexOf(SolutionConstants.kLabelSeperator);
            
            var clientName = (((posLabelSeparator > 0) && (posLabelSeparator < (clientLabel.Name.Length - 1))) 
                ? clientLabel.Name.Substring(posLabelSeparator + 1) 
                : clientLabel.Name)
                .Trim();

            return clientName;
        }

        #endregion
    }
}