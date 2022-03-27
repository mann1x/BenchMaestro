using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics.Eventing.Reader;
using System.Text.RegularExpressions;
using System.Management;
using GongSolutions.Wpf.DragDrop;
using System.Collections.ObjectModel;
using System.Windows;

namespace BenchMaestro
{

    class RunsViewModel : IDropTarget
    {
        public ObservableCollection<string> Schools { get; private set; }


        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            Trace.WriteLine("DRAG");
        }
        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            Trace.WriteLine("DROP");
        }
    }

    /*
       class RunsViewModel
       {
           static readonly string eventLogName = "System";

           private IList<Event> _EventList;

           public RunsViewModel()
           {

               _EventList = new List<Event> { };

               string evtquery = "*[System/Provider/@Name=\"Microsoft-Windows-WHEA-Logger\"]";

               try { 
                   EventLogQuery elq = new EventLogQuery(eventLogName, PathType.LogName, evtquery);
                   elq.Session = new EventLogSession();

                   elq.ReverseDirection = true;

                   using (EventLogReader elr = new EventLogReader(elq))
                   {
                       EventRecord ev;
                       while ((ev = elr.ReadEvent()) != null) {
                           string _reportedBy = "N/A";
                           string _source = "N/A";
                           string _type = "N/A";
                           string _processor = "N/A";
                           string _message = "N/A";
                           string _rawmessage = "";
                           string _errorSource = "";
                           string _apicId = "";
                           string _mcaBank = "";
                           string _mciStat = "";
                           string _mciAddr = "";
                           string _mciMisc = "";
                           string _errorType = "";
                           string _transactionType = "";
                           string _participation = "";
                           string _requestType = "";
                           string _memorIO = "";
                           string _memHierarchyLvl = "";
                           string _timeout = "";
                           string _operationType = "";
                           string _length = "";
                           string _rawData = "";

                           if (ev.FormatDescription() != null)
                           {
                               _rawmessage = ev.FormatDescription();
                               using (var sreader = new StringReader(_rawmessage))
                               {
                                   _message = sreader.ReadLine();
                               }
                               string pattern = @"Reported by component: (?<reportedby>.*)[\n\r]^Error Source: (?<source>.*)[\n\r]^Error Type: (?<type>.*)[\n\r]^Processor APIC ID: (?<processor>.*)[\n\r]";
                               Regex rgx = new Regex(pattern, RegexOptions.Multiline);
                               Match m = rgx.Match(_rawmessage);
                               if (m.Success)
                               {
                                   string[] fields = rgx.GetGroupNames();
                                   foreach (var name in fields)
                                   {
                                       Group grp = m.Groups[name];
                                       if (name == "reportedby" && grp.Value.Length > 0) _reportedBy = grp.Value.TrimEnd('\r', '\n'); ;
                                       if (name == "source" && grp.Value.Length > 0) _source = grp.Value.TrimEnd('\r', '\n'); ;
                                       if (name == "type" && grp.Value.Length > 0) _type = grp.Value.TrimEnd('\r', '\n'); ;
                                       if (name == "processor" && grp.Value.Length > 0) _processor = grp.Value.TrimEnd('\r', '\n'); ;
                                   }
                               }
                           }

                           try
                           {
                               XmlSerializer serializer = new XmlSerializer(typeof(XmlArray),
                                   "http://schemas.microsoft.com/win/2004/08/events/event");

                               StringReader xreader = new StringReader(ev.ToXml());
                               var xmlevt = (XmlArray)serializer.Deserialize(xreader);
                               //Trace.WriteLine("XmlDeserialize EventData " + xmlevt.EventData.Length.ToString());
                               foreach (XmlData evtattrib in xmlevt.EventData)
                               {
                                   switch (evtattrib.Name.ToString())
                                   {
                                       case "ErrorSource":
                                           _errorSource = evtattrib.Value.ToString();
                                           break;
                                       case "ApicId":
                                           _apicId = evtattrib.Value.ToString();
                                           break;
                                       case "MCABank":
                                           _mcaBank = evtattrib.Value.ToString();
                                           break;
                                       case "MciStat":
                                           _mciStat = evtattrib.Value.ToString();
                                           break;
                                       case "MciAddr":
                                           _mciAddr = evtattrib.Value.ToString();
                                           break;
                                       case "MciMisc":
                                           _mciMisc = evtattrib.Value.ToString();
                                           break;
                                       case "ErrorType":
                                           _errorType = evtattrib.Value.ToString();
                                           break;
                                       case "TransactionType":
                                           _transactionType = evtattrib.Value.ToString();
                                           break;
                                       case "Participation":
                                           _participation = evtattrib.Value.ToString();
                                           break;
                                       case "RequestType":
                                           _requestType = evtattrib.Value.ToString();
                                           break;
                                       case "MemorIO":
                                           _memorIO = evtattrib.Value.ToString();
                                           break;
                                       case "MemHierarchyLvl":
                                           _memHierarchyLvl = evtattrib.Value.ToString();
                                           break;
                                       case "Timeout":
                                           _timeout = evtattrib.Value.ToString();
                                           break;
                                       case "OperationType":
                                           _operationType = evtattrib.Value.ToString();
                                           break;
                                       case "Length":
                                           _length = evtattrib.Value.ToString();
                                           break;
                                       case "RawData":
                                           _rawData = evtattrib.Value.ToString();
                                           break;
                                       default:
                                           break;
                                   }
                                   //Trace.WriteLine("XmlDeserialize EventData " + evtattrib.Name.ToString());
                                   //Trace.WriteLine("XmlDeserialize EventData " + evtattrib.Value.ToString());
                               }
                           }
                           catch (Exception ex)
                           {
                               Trace.WriteLine("XmlDeserialize Exception " + ex.ToString());
                               Trace.WriteLine("XmlDeserialize Exception " + ev.ToXml());
                           }

                           _EventList.Add(new Event
                           {
                               recordId = (long)ev.RecordId,
                               severity = (int)ev.Level,
                               severityDesc = ev.LevelDisplayName,
                               eventId = ev.Id,
                               timestamp = (DateTime)ev.TimeCreated,
                               reportedBy = _reportedBy,
                               source = _source,
                               type = _type,
                               processor = _processor,
                               message = _message,
                           });
                       }

                   }
               }
               catch
               {
                   Trace.WriteLine("EventReader Exception");
               }

           }

           public IList<Event> Events
           {
               get { return _EventList; }
               set { _EventList = value; }
           }

           private ICommand mUpdater;
           public ICommand UpdateCommand
           {
               get
               {
                   if (mUpdater == null)
                       mUpdater = new Updater();
                   return mUpdater;
               }
               set
               {
                   mUpdater = value;
               }
           }

           private class Updater : ICommand
           {
               #region ICommand Members  

               public bool CanExecute(object parameter)
               {
                   return true;
               }

               public event EventHandler CanExecuteChanged;

               public void Execute(object parameter)
               {

               }

               #endregion
           }
       }
    */

}
