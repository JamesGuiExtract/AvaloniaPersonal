namespace ExtractLicenseUI.DatFileUtility
{
    /// <summary>
    /// Used for loading components into the database.
    /// </summary>
    class ComponentModel
    {
        public int ComponentID { get; set; }
        public string ComponentName { get; set; }

        public override bool Equals(object obj)
        {
            return obj is ComponentModel model &&
                   ComponentID == model.ComponentID;
        }

        public override int GetHashCode()
        {
            var hashCode = 129498792;
            hashCode = hashCode * -1521134295 + ComponentID.GetHashCode();
            return hashCode;
        }
    }
}
