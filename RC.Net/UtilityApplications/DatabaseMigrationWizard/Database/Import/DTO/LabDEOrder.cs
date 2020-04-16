using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using static System.FormattableString;

namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class LabDEOrder
    {
        public string OrderNumber { get; set; }

        public string OrderCode { get; set; }

        public string PatientMRN { get; set; }

        public string ReceivedDateTime { get; set; }

        public string OrderStatus { get; set; }

        public string ReferenceDateTime { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Naming violations are a result of acronyms in the database.")]
        public string ORMMessage { get; set; }

        public string EncounterID { get; set; }

        public string AccessionNumber { get; set; }

        public Guid Guid { get; set; }

        public override bool Equals(object obj)
        {
            return obj is LabDEOrder order &&
                   OrderNumber == order.OrderNumber &&
                   OrderCode == order.OrderCode &&
                   PatientMRN == order.PatientMRN &&
                   DateTime.Parse(ReceivedDateTime, CultureInfo.InvariantCulture) == DateTime.Parse(order.ReceivedDateTime, CultureInfo.InvariantCulture) &&
                   OrderStatus == order.OrderStatus &&
                   DateTime.Parse(ReferenceDateTime, CultureInfo.InvariantCulture) == DateTime.Parse(order.ReferenceDateTime, CultureInfo.InvariantCulture) &&
                   ORMMessage == order.ORMMessage &&
                   EncounterID == order.EncounterID &&
                   AccessionNumber == order.AccessionNumber &&
                   Guid.Equals(order.Guid);
        }

        public override int GetHashCode()
        {
            var hashCode = 971177235;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(OrderNumber);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(OrderCode);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PatientMRN);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ReceivedDateTime);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(OrderStatus);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ReferenceDateTime);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ORMMessage);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(EncounterID);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AccessionNumber);
            hashCode = hashCode * -1521134295 + Guid.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return Invariant($@"(
                        '{OrderNumber}'
                        , '{OrderCode}'
                        , {(PatientMRN == null ? "NULL" : "'" + PatientMRN.Replace("'", "''") + "'")}
                        , '{ReceivedDateTime}'
                        , '{OrderStatus}'
                        , {(ReferenceDateTime == null ? "NULL" : "'" + ReferenceDateTime + "'")}
                        , {(ORMMessage == null ? "NULL" : "CONVERT(XML, N'" + ORMMessage.Replace("'", "''") + "')")}
                        , {(EncounterID == null ? "NULL" : "'" + EncounterID + "'")}
                        , {(AccessionNumber == null ? "NULL" : "'" + AccessionNumber + "'")}
                        , '{Guid}'
                    )");
        }
    }
}
