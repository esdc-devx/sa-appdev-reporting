using System.Collections.Generic;

namespace esdc_sa_appdev_reporting_api.Constants
{
    public sealed class SolutionConstants
    {
        #region --- Enums -----------------------------------------------------
        /*
        public enum ActionType
        {
            PastDue,
            NotStarted,
            InProgress,
            Completed
        }
        */
        #endregion


        #region --- Sets ------------------------------------------------------

        public static class TrelloLists
        {
            public const string Backlog = "Backlog";
            public const string Committed = "Sprint Backlog";
            public const string InProgress = "In Progress";
            public const string OnHold = "On Hold / Blocked";
            public const string Done = "Done";
        }

        
        public static class TrelloLabelCategory
        {
            public const string Client = "green";
            public const string Epic = "red";
            public const string DevX = "blue";
            public const string Administration = "sky";
            public const string Feature = "purple";
            public const string Team = "black";
        }

        #endregion


        #region --- Complex ---------------------------------------------------

        public static class TrelloListIndexMap
        {
            // { index, list }

            public static readonly Dictionary<short, string> Map = new Dictionary<short, string>()
                {
                    {1, SolutionConstants.TrelloLists.Backlog},
                    {2, SolutionConstants.TrelloLists.Committed},
                    {3, SolutionConstants.TrelloLists.InProgress},
                    {4, SolutionConstants.TrelloLists.OnHold},
                    {5, SolutionConstants.TrelloLists.Done}
                };
        }

        #endregion


        #region --- Single Values ---------------------------------------------

        public const string kTrelloAppKey = "534001bbfeb9302f0f62fd6263f80567";
        public const string kTrelloUserSecret = "861101898508498761cd2952b258926b568326994f44ef23fd85ca3685667525";
        public const string kTrelloUserToken = "9a4b5cc8411e7380cf8fc392341f118e2f32869ac50a134fb9bd81f43fb0fe62";
        public const string kTrelloBoardId = "5cdf08913ae8753993cfdd9c";

        public const string kLabelSeperator = ":";

        public const string kSelectListKeyEmpty = "";
        public const string kSelectListKeyAll = "ALL";
        public const string kSelectListKeyUnknown = "UNKNOWN";

        #endregion
    }
}