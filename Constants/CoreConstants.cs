using System;

namespace esdc_sa_appdev_reporting_api.Constants
{
    public sealed class CoreConstants
    {
        #region --- Enums -----------------------------------------------------

        [Flags]
        public enum OpFlag
        {
            None = 0x0,
            Success = 0x1,
            Halted = 0x2,
            Exception = 0x4,
            Error = 0x8,
            Warning = 0x10,
            Notice = 0x20,
            ErrValidation = 0x40,
            ErrBusiness = 0x80,
            ErrSecurity = 0x100,
            ErrProcess = 0x200,
        }

        [Flags]
        public enum Lang
        {
            None = 0x0,
            En = 0x1,
            Fr = 0x2
        }


        [Flags]
        public enum WcFilter
        {
            // Wild Card Filters { %x, %x%, x% }

            None = 0x0,
            StartsWith = 0x1,
            Contains = 0x2,
            EndsWith = 0x4
        }


        [Flags]
        public enum RangeLimit
        {
            None = 0x0,
            Min = 0x1,
            Max = 0x2
        }


        [Flags]
        public enum SortOrder
        {
            None = 0x0,
            Asc = 0x1,
            Desc = 0x2
        }


        public enum ConnectivityMode
        {
            Online,
            Offline
        }
        

        public enum EntityOperation
        {
            None,
            InitCreate,
            Add,
            Modify,
            Delete,
            Merge,
            Purge
        }


        public enum TreeResultMode
        {
            NodeTraverse,
            NodeConcat
        }

        #endregion


        #region --- Sets ------------------------------------------------------

        public static class CultureIdent
        {
            public const string En = "en-CA";
            public const string Fr = "fr-CA";
        }


        public static class LangThreeLetter
        {
            public const string En = "eng";
            public const string Fr = "fra";
        }


        public static class LangIdent
        {
            public const string En = "en";
            public const string Fr = "fr";
        }


        public static class YesNoIdent
        {
            public const string Yes = "Y";
            public const string No = "N";
        }


        public static class RegEx
        {
            public const string Integer = "^[0-9]*$";
            public const string NullableInteger = "^[0-9]*$|^$";  
            public const string PostalCode = "[abceghjklmnprstvxyABCEGHJKLMNPRSTVXY][0-9][abceghjklmnprstvwxyzABCEGHJKLMNPRSTVWXYZ][ ]?[0-9][abceghjklmnprstvwxyzABCEGHJKLMNPRSTVWXYZ][0-9]";
            public const string TelephoneNo = "[1]?[ ]?[\\(]{0,1}([0-9]){3}[\\)]{0,1}[ ]?([0-9]){3}[ ]?[-]?[ ]?([0-9]){4}[ ]?([xX][ ]?[0-9]{1,5})?";
            public const string Email = "([a-zA-Z0-9_\\-\\.]+)@((\\[[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.)|(([a-zA-Z0-9\\-]+\\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\\]?)";
            public const string SecurityUserName = "[a-zA-Z]{1}[a-zA-Z0-9_]*";
            public const string SecurityPassword = ".*(?=.{8,20})(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z]).*";
        }


        public static class Formats
        {
            public const string DateIso = "yyyy-MM-dd";
            public const string DtmIso = "yyyy-MM-dd HH:mm:ss";
            public const string DtmZuluIso = "yyyy-MM-ddTHH:mm:ss.fffZ";
        }


        public static class DefaultValues
        {
            public const int Numeric = 0;
            public const int NonZero = -1;
        }


        public static class Separators
        {
            public const string Default = "|";
            public const string OutputLabel = " / ";
        }


        public static class FileFilters
        {
            public const string FileFilterDefaultAll = "All / tout (*.*)|*.*";
            public const string FileFilterLibraryDocument = "Valid / valides|*.pdf;*.xls;*.doc;*.docx|pdf (*.pdf)|*.pdf|xls (*.xls)|*.xls|doc (*.doc)|*.doc|doc (*.docx)|*.docx";
            public const string FileFilterLibraryImage = "Valid / valides|*.jpg;*.png;*.gif;*.tiff|jpg (*.jpg)|*.jpg|png (*.png)|*.png|gif (*.gif)|*.gif|tiff (*.tiff)|*.tiff";
            public const string FileFilterRDIMSsupportedFiles = "MS Office |*.doc;*.docx;*.xls;*.xlsx;*.ppt;*.pptx*.pptx;*.vsd;*.mpp|PDF |*.pdf|Images files|*.jpg;*.png";
        }

        #endregion
    }
}
