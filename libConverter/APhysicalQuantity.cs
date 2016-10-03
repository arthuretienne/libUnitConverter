
namespace libConverter {
    public class APhysicalQuantity {

        private PhysicalQuantity PQ = new PhysicalQuantity();
        
        private string _unit;
        
        public double Value { get; set; }
        public string Unit {
            get {
                return _unit;
            }
            set {
                if ( PQ.checkMyUnit( value ) )
                    _unit = value;
                else
                    throw new System.Exception();
            }
        }
        
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
