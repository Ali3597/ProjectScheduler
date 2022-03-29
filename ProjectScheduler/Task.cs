using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace ProjectScheduler
{
    public class Task
    {
        public Task(string title) : this(title, IClock.Current.Now) { }

        public Task(string title, DateTime start) : this(title, start, null) { }

        public Task(string title, DateTime start, DateTime end) : this(title, start, (DateTime?)end)
        {
            CheckEnd(end);
        }

        public Guid Id { get; private init; }

        public string Title
        {
            get => title;
            set
            {
                if (value == null)
                {
                    throw new NullReferenceException("Title cannot be null");
                }
                title = value;
            }
        }

        public bool IsStarted => start < IClock.Current.Now;

        public bool HasEnd => end.HasValue;

        public bool HasDuration => IsStarted || HasEnd;

        public TimeSpan Duration
        {
            get
            {
                if (!HasDuration)
                {
                    throw new InvalidOperationException("Future not ended task has no duration");
                }
                return (HasEnd ? End : IClock.Current.Now) - Start;
            }
        }
        public DateTime Start
        {
            get => start;
            set
            {
                if (HasEnd)
                {
                    end = value + Duration;
                }
                start = value;
            }
        }

        public DateTime End
        {
            get => HasEnd
                ? end.Value
                : throw new InvalidOperationException("Not ended task has no end date");

            set => end = CheckEnd(value);
        }

        public static Task FromDb(IDataReader reader)
        {
            var id = reader.GetGuid(0);
            var title = reader.GetString(1);
            var start = reader.GetDateTime(2);

            return new Task(
                id, title, start,
                reader.IsDBNull(3) ? null : reader.GetDateTime(3)
            );
        }

        public static Task FromXml(XmlReader reader)
        {
            do
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Comment:
                        break;
                    case XmlNodeType.Element:
                        if (reader.Name != "task")
                        {
                            throw new FormatException($"Unexpected {reader.Name} element");
                        }
                        var values = new string[TaskProperties.Length];

                        for (int i = 0; i < TaskProperties.Length; i++)
                        {
                            values[i] = reader.GetAttribute(TaskProperties[i]);
                        }
                        var nullIndex = Array.IndexOf(values, null);

                        if (0 <= nullIndex && nullIndex <= 2)
                        {
                            throw new FormatException("Task must have an id, title and a start date");
                        }
                        return FromValues(values);
                    default:
                        throw new FormatException("Xml Element expected");
                }
            }
            while (reader.Read());
            throw new InvalidOperationException("Unexpected end of XML");
        }

        public void ToXml(IXmlWriter writer)
        {
            writer.WriteStartElement("task");
            writer.WriteAttributeString("id", Id.ToString());
            writer.WriteAttributeString("title", Title);
            writer.WriteAttributeString("start", Start.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("end",
                HasEnd ? End.ToString(CultureInfo.InvariantCulture) : null
            );
            writer.WriteEndElement();
        }

        public static Task FromCsv(TextReader reader, char sep = ';')
        {
            var parts = reader.ReadLine().Split(sep);

            if (parts.Length < 3)
            {
                throw new FormatException("CSV line must have at least id;name;start_date");
            }
            return FromValues(parts);
        }

        public void ToCsv(ITextWriter writer)
        {
            StringBuilder sb = new($"{Id};{Title};{Start}");

            if (HasEnd)
            {
                sb.Append($";{End}");
            }
            writer.WriteLine(sb.ToString());
        }

        #region Privates
        private DateTime start;
        private DateTime? end;
        private string title;

        private Task(Guid id, string title, DateTime start, DateTime? end)
        {
            Id = id;
            Title = title;
            Start = start;
            this.end = end;
            if (end.HasValue)
            {
                CheckEnd(end.Value);
            }
        }

        private Task(string title, DateTime start, DateTime? end) : this(IGuid.Current.id, title, start, end)
        { }

        private DateTime CheckEnd(DateTime newEnd)
        {
            if (newEnd <= start)
            {
                throw new ArgumentOutOfRangeException("End date cannot before or exactly the start date");
            }
            return newEnd;
        }

        private static readonly string[] TaskProperties = { "id", "title", "start", "end" };

        private static Task FromValues(string[] values)
        {
            return new Task(
                Guid.Parse(values[0]),
                values[1],
                DateTime.Parse(values[2], CultureInfo.InvariantCulture),
                values.Length == 4 && values[3] != null
                    ? DateTime.Parse(values[3], CultureInfo.InvariantCulture)
                    : null
            );
        }

        #endregion
    }
}
