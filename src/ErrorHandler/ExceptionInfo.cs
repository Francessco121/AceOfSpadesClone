using System;
using System.Text;

/* ExceptionInfo.cs
 * Ethan Lafrenais
*/

namespace ErrorHandler
{
    public class ExceptionInfo
    {
        public string Stack { get; set; }
        public string Site { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
        public string ExceptionType { get; set; }
        public ExceptionInfo InnerException { get; set; }

        public ExceptionInfo() { }

        public ExceptionInfo(Exception e)
        {
            Stack = e.StackTrace;
            Site = e.TargetSite.ToString();
            Message = e.Message;
            Source = e.Source;
            ExceptionType = e.GetType().FullName;
            if (e.InnerException != null)
                InnerException = new ExceptionInfo(e.InnerException);
        }

        public void FillNullValues()
        {
            if (Stack == null) Stack = "";
            if (Site == null) Site = "";
            if (Message == null) Message = "";
            if (Source == null) Source = "";
            if (ExceptionType == null) ExceptionType = "";
        }

        public string ToCommandArgString()
        {
            StringBuilder sb = new StringBuilder();
            WriteArg(sb, "site", Site);
            WriteArg(sb, "message", Message);
            WriteArg(sb, "source", Source);
            WriteArg(sb, "exceptionType", ExceptionType);
            WriteArg(sb, "stack", Stack); // Write stack last because it can be absolutly huge

            if (InnerException != null)
            {
                WriteArg(sb, "beginInner", "true");
                WriteArg(sb, "stack", InnerException.Stack);
                WriteArg(sb, "site", InnerException.Site);
                WriteArg(sb, "message", InnerException.Message);
                WriteArg(sb, "source", InnerException.Source);
                WriteArg(sb, "exceptionType", InnerException.ExceptionType);
            }
            else
                WriteArg(sb, "beginInner", "false");

            return sb.ToString();
        }

        void WriteArg(StringBuilder sb, string name, string value)
        {
            sb.AppendFormat("-{0}:\"{1}\" ", name, value);
        }
    }
}
