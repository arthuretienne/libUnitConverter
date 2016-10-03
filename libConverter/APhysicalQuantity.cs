
namespace libConverter {
    public class APhysicalQuantity {

        private double _value;
        private string _unit;
        
        public double Value { get; set; }
        public string Unit { get; set; }

        private PhysicalQuantity PQ = new PhysicalQuantity();

        /// <summary>
        /// Get a converted value from actual unit to newUnit
        /// </summary>
        /// <param name="inNewUnit">Name of the unit to convert into</param>
        /// <returns></returns>
        public double GetValue(string inNewUnit) {
            return PQ.convertUnit( Value , Unit , inNewUnit );
        }
    }
}
