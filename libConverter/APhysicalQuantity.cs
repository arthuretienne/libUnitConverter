using System.Runtime.InteropServices;

namespace libUnitConverter {
    public interface IAPhysicalQuantity {
        double Value { get; set; }
        string Unit { get; set; }
        double GetValue(string inThisUnit);
    }
    [ClassInterface(ClassInterfaceType.None)]
    public class APhysicalQuantity : IAPhysicalQuantity {

        private PhysicalQuantity PQ = new PhysicalQuantity();
        
        private string _unit;
        
        public double Value { get; set; }
        public string Unit {
            get {
                return _unit;
            }
            set {
                if ( PQ.checkMyUnit(value) ) {
                    _unit = value;
                } else {
                    throw new System.Exception("Not supported unit : " + _unit);
                }
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
