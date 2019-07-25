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
        public async Task<List<List<string>>> GetSummaryReportResults()
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

            var results = new List<List<string>>();

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
                // Get the first green label of the card (should only be only, but just in case, we'll grab the first).
                var subCategoryLabel = result.CardLabels.FirstOrDefault(x => x.Color == SolutionConstants.TrelloLabelCategory.Client);

                // Get the label sub-category (meaning, remove the prefix, i.e. "Client: ")
                var posLabelSeparator = subCategoryLabel.Name.IndexOf(SolutionConstants.kLabelSeperator);
                
                var clientKey = (((posLabelSeparator > 0) && (posLabelSeparator < (subCategoryLabel.Name.Length - 1))) 
                    ? subCategoryLabel.Name.Substring(posLabelSeparator + 1) 
                    : subCategoryLabel.Name)
                    .Trim();

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

                results.Add(trelloClientValues);
            }

            // Order the clients alphabetically
            results = results.OrderBy(x => x.FirstOrDefault()).ToList();
            
            // Add the first result row.
            var firstResultSetValues = new List<string>();

            firstResultSetValues.Add(" ");
            firstResultSetValues.Add(SolutionConstants.TrelloLists.Backlog);
            firstResultSetValues.Add(SolutionConstants.TrelloLists.Committed);
            firstResultSetValues.Add(SolutionConstants.TrelloLists.InProgress);
            firstResultSetValues.Add(SolutionConstants.TrelloLists.OnHold);
            firstResultSetValues.Add(SolutionConstants.TrelloLists.Done);

            results.Insert(0, firstResultSetValues);

            return results;
        }


        public async Task<List<GeneralReportResult>> GetGeneralReportResults()
        {
            // 1. Gather all cards
            // 2. Compute date started and completed
            // 3. Compute number of days on hold

            var results = new List<GeneralReportResult>();

            var trelloLists = await this.GetTrelloLists();
            var trelloLabels = await this.GetTrelloLabels();
            var trelloMembers = await this.GetTrelloMembers();
            var trelloCards = await this.GetTrelloCards();
            var trelloCardMoveActions = await this.GetTrelloCardMoveActions();

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
                if ((result.CardCurrentListTitle != SolutionConstants.TrelloLists.Backlog) ||
                    (result.CardCurrentListTitle != SolutionConstants.TrelloLists.Committed))
                {
                    var action = trelloCardMoveActions
                        .Where
                        (   
                            x => 
                                (x.data.card.id == card.id) && 
                                (x.data.listAfter.name == SolutionConstants.TrelloLists.InProgress)
                        )
                        .FirstOrDefault();

                    result.CardDateStarted = action?.date;
                }

                // Only cards in done are considered completed.
                if (result.CardCurrentListTitle != SolutionConstants.TrelloLists.Done)
                {
                    var action = trelloCardMoveActions
                        .Where(x => (x.data.card.id == card.id) && x.data.listAfter.name == SolutionConstants.TrelloLists.Done)
                        .LastOrDefault();

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

                return null;
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
                        + "&token=" + SolutionConstants.kTrelloUserToken;

                    var response = await http.GetAsync(url);

                    response.EnsureSuccessStatusCode();

                    var jsonResult = await response.Content.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<List<TrelloLabelsDto>>(jsonResult);
                }
                catch (HttpRequestException httpRequestException)
                {
                    Console.WriteLine($"Error in GetTrelloLabels: {httpRequestException.Message}");
                }

                return null;
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

                return null;
            }
        }


        public async Task<List<TrelloCardDto>> GetTrelloCards()
        {
            using (var http = new HttpClient())
            {
                try
                {
                    var url = "https://api.trello.com/1/boards/" + SolutionConstants.kTrelloBoardId + "/cards/"
                        + "?key=" + SolutionConstants.kTrelloAppKey 
                        + "&token=" + SolutionConstants.kTrelloUserToken;

                    var response = await http.GetAsync(url);

                    response.EnsureSuccessStatusCode();

                    var jsonResult = await response.Content.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<List<TrelloCardDto>>(jsonResult);
                }
                catch (HttpRequestException httpRequestException)
                {
                    Console.WriteLine($"Error in GetTrelloCards: {httpRequestException.Message}");
                }

                return null;
            }
        }


        // https://stackoverflow.com/questions/51777063/how-can-i-get-all-actions-for-a-board-using-trellos-rest-api

        public async Task<List<TrelloActionDto>> GetTrelloCardMoveActions()
        {
            using (var http = new HttpClient())
            {
                try
                {
                    var url = "https://api.trello.com/1/boards/" + SolutionConstants.kTrelloBoardId + "/actions/"
                        + "?key=" + SolutionConstants.kTrelloAppKey 
                        + "&token=" + SolutionConstants.kTrelloUserToken
                        //+ "&before=2019-07-01"
                        //+ "&since=2019-06-01"
                        + "&filter=updateCard" 
                        + "&limit=1000";

                    var response = await http.GetAsync(url);

                    response.EnsureSuccessStatusCode();

                    var jsonResult = await response.Content.ReadAsStringAsync();

                    var list = JsonConvert.DeserializeObject<List<TrelloActionDto>>(jsonResult);

                    return list.Where(x => (x.IsListTransfer() == true)).ToList();
                }
                catch (HttpRequestException httpRequestException)
                {
                    Console.WriteLine($"Error in GetTrelloCardMoveActions: {httpRequestException.Message}");
                }

                return null;
            }
        }
    }
}