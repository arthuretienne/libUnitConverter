using System.Collections.Generic;
using System.Reflection;
using System.Data;
using System;

namespace libUnitConverter {
    public class PhysicalQuantity
    {
        #region Get/Set
        public List<string> PressureUnits {
            get {
                return new List<string>(_pressureUnitTable.Keys);
            }
        }
        public List<string> TimeUnits {
            get {
                return new List<string>(_timeUnitTable.Keys);
            }
        }
        public List<string> TemperatureUnits {
            get {
                return new List<string>(_temperatureUnitTable.Keys);
            }
        }
        public List<string> VolumeUnits {
            get {
                return new List<string>(_volumeUnitTable.Keys);
            }
        }

        public List<string> VolumeFlowUnits {
            get {
                return _volumeFlowUnitTable;
            }
        }
        #endregion

        #region Convert Table
        private static readonly List<string> allowedPhysicalQuantity = new List<string> {
            "volume","temperature","pressure","time","volumeFlow","massFlow"
        };

        private static readonly Dictionary<string,string> _pressureUnitTable = new Dictionary<string,string>
        {
            { "Pa", "1" },
            { "hPa", "100" }, //1hPa = 100Pa
            { "kPa", "1000" }, //1kPa = 1000Pa
            { "mbar", "100" }, //1mbar = 1hPa
            { "bar", "1e5" }, //1bar = 1E5 Pa
            { "Torr", "133.322" }, //1Torr = 133.322Pa
            { "atm", "101325" } //1atm = 101325Pa
        };
        private static readonly Dictionary<string,string> _timeUnitTable = new Dictionary<string,string>
        {
            { "s", "1" },
            { "min", "60" }, //1min = 60s
            { "h", "3600" } //1h = 3600s
        };
        private static readonly Dictionary<string,string> _volumeUnitTable = new Dictionary<string,string>
        {
            { "L", "1" },
            { "mL", "0.001" }, //1mL = 0.001L
            { "m3", "1000" }, //1m3 = 1000L
            { "gal US", "3.785411784" }, //1 gallon u.s. = 3.785411784 L
            { "gal GB", "4.54609" } //1 gallon GB/Imp = 4.54609L
        };

        private static readonly Dictionary<string,List<string>> _temperatureUnitTable = new Dictionary<string,List<string>> {
            { "°C",
                    new List<string> {
                                        "1", //Base unit
                                        "1"
                                    }
            },
            { "K",
                    new List<string> {
                                        "={x}-273,15", //K => °C
                                        "={x}+273,15" //°C => K
                                    }
            },
            { "°F",
                    new List<string> {
                                        "=({x}-32)/1.8", //°F => °C
                                        "={x}*1.8+32" //°C => °F
                                    }
            }
        };

        private List<string> _volumeFlowUnitTable = new List<string>();
        #endregion

        #region Class constructors
        public PhysicalQuantity() {
            InitializeCombinedUnits();
        }
        private void InitializeCombinedUnits() {
            //volumeFlow = Volume units / time unit
            foreach (string volumeKey in _volumeUnitTable.Keys) {
                foreach (string timeKey in _timeUnitTable.Keys) {
                    _volumeFlowUnitTable.Add(volumeKey + "/" + timeKey);
                }
            }
        }
        #endregion

        /// <summary>
        /// Determine in which category belongs the unit
        /// </summary>
        /// <param name="whichUnit">The unit that we have to check</param>
        /// <returns></returns>
        public string determinePhysicalQuantity(string whichUnit) {
            if (_pressureUnitTable.ContainsKey(whichUnit)) {
                return "pressure";
            } else if (_timeUnitTable.ContainsKey(whichUnit)) {
                return "time";
            } else if (_volumeUnitTable.ContainsKey(whichUnit)) {
                return "volume";
            } else if (_temperatureUnitTable.ContainsKey(whichUnit)) {
                return "temperature";
            } else if (_volumeFlowUnitTable.Contains(whichUnit)) {
                return "volumeFlow";
            }
            else {
                return "unknown";
            }
        }

        #region Coefficients functions
        private string pressureCoefficient(string forUnit,bool reverse) {
            return _pressureUnitTable[forUnit];
        }
        private string timeCoefficient(string forUnit,bool reverse) {
            return _timeUnitTable[forUnit];
        }
        private string volumeCoefficient(string forUnit,bool reverse) {
            return _volumeUnitTable[forUnit];
        }
        private string temperatureCoefficient(string forUnit,bool reverse) {
            if (reverse) {
                return _temperatureUnitTable[forUnit][1];
            } else {
                return _temperatureUnitTable[forUnit][0];
            }
        }
        #endregion

        #region Converter himself
        /// <summary>
        /// Convert a value from one unit to another
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="fromUnit">Current unit</param>
        /// <param name="toUnit">Target unit</param>
        /// <returns></returns>
        public double convertUnit(double value, string fromUnit, string toUnit) {
            //Easy one =) 
            if (fromUnit == toUnit) {
                return value;
            } else {
                //Can't convert from carrots to potatoes...
                if (determinePhysicalQuantity(fromUnit) == determinePhysicalQuantity(toUnit)) {

                    //Contains divided units, split string
                    // Do conversion of each single unit on numerator
                    // Invert (1/x)
                    // Do conversion of each unit in denominator
                    // It's OK
                    fromUnit =  "bar.°C." + fromUnit;
                    toUnit = "mbar.K." + toUnit;
                    if (fromUnit.IndexOf("/") > 0) {
                        string[] divSeparator = new string[] { "/" };
                        string[] multSeparator = new string[] { "." };
                        string[] fromDivUnits = fromUnit.Split(divSeparator,StringSplitOptions.None);
                        string[] toDivUnits = toUnit.Split(divSeparator,StringSplitOptions.None);

                        for (int i = 0; i <= fromDivUnits.GetUpperBound(0); i++) {

                            string[] fromMultUnits = fromDivUnits[i].Split(multSeparator,StringSplitOptions.None);
                            string[] toMultUnits = toDivUnits[i].Split(multSeparator,StringSplitOptions.None);
                            
                            for (int j = 0; j <= fromMultUnits.GetUpperBound(0); j++) {
                                value = unitConverter(value,fromMultUnits[j],toMultUnits[j]);
                            }

                            value = 1 / value;

                        }

                        return value;
                    } else if (fromUnit.IndexOf(".") > 0) {
                        //Only mutliplied units
                        return -2;
                    } else {
                        return unitConverter(value,fromUnit,toUnit);
                    }

                } else {
                    return -1;
                }
            }
        }

        private double unitConverter(double value, string fromUnit, string toUnit) {

            DataTable dt = new DataTable();
            string methodName = determinePhysicalQuantity(fromUnit) + "Coefficient";

            //try {

                MethodInfo mi = this.GetType().GetMethod(methodName,BindingFlags.NonPublic | BindingFlags.Instance);
                double valueInRefUnit;
                double valueInTargetUnit;

                //If expression not starting by "=" then do a multiplication, else compute that !
                string firstCoef = mi.Invoke(this,new object[] { fromUnit,false }).ToString();
                if (firstCoef.IndexOf("=") >= 0) {
                    firstCoef = firstCoef.Replace("{x}",value.ToString());
                    firstCoef = firstCoef.Replace("=","");
                    firstCoef = firstCoef.Replace(",",".");

                    valueInRefUnit = double.Parse(dt.Compute(firstCoef.ToString(),"").ToString());
                } else {
                    valueInRefUnit = value * double.Parse(mi.Invoke(this,new object[] { fromUnit,false }).ToString().Replace(".",","));
                }

                string secondCoef = mi.Invoke(this,new object[] { toUnit,true }).ToString();
                if (secondCoef.IndexOf("=") >= 0) {
                    secondCoef = secondCoef.Replace("{x}",valueInRefUnit.ToString());
                    secondCoef = secondCoef.Replace("=","");
                    secondCoef = secondCoef.Replace(",",".");

                    valueInTargetUnit = double.Parse(dt.Compute(secondCoef,null).ToString());
                } else {
                    valueInTargetUnit = valueInRefUnit / double.Parse(mi.Invoke(this,new object[] { toUnit,true }).ToString().Replace(".",","));
                }

                dt.Dispose();
                return valueInTargetUnit;

            //} catch (Exception ex) {
            //    System.Windows.Forms.MessageBox.Show(ex.Message,"Error",System.Windows.Forms.MessageBoxButtons.OK,System.Windows.Forms.MessageBoxIcon.Error);
            //    return -1;
            //}
        }
        #endregion

        #region Other cool stuffs
        /// <summary>
        /// Go through your list of unit and remove those that the application doesn't known
        /// </summary>
        /// <param name="myList">Your list of unit</param>
        /// <param name="forPhysicalQty">The physical quantity (pressure, temperature,...)</param>
        /// <returns></returns>
        public List<string> checkMyListOfUnit(List<string> myList,string forPhysicalQty) {
            if (allowedPhysicalQuantity.Contains(forPhysicalQty)) {
                
                string methodName = forPhysicalQty.Substring(0,1).ToUpper() + forPhysicalQty.Substring(1) + "Units";

                PropertyInfo pi = this.GetType().GetProperty(methodName,BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                List<string> knownUnits = (List<string>) pi.GetValue(this);

                for (int i = 0; i < myList.Count; i++) {
                    if (!knownUnits.Contains(myList[i])) {
                        myList.RemoveAt(i);
                    }
                }

            } else {
                myList.Add("Physical quantity not supported");
            }

            return myList;
        }

        /// <summary>
        /// Check if your unit is supported by the converter
        /// </summary>
        /// <param name="unit">Your unit to check</param>
        /// <param name="forPhysicalQty">What kind of unit it is (pressure, temperature, flowrate, ...)</param>
        /// <returns></returns>
        public bool checkMyUnit(string unit) {
            string forPhysicalQty = determinePhysicalQuantity( unit );

            if ( allowedPhysicalQuantity.Contains( forPhysicalQty ) ) {
                
                string methodName = forPhysicalQty.Substring(0,1).ToUpper() + forPhysicalQty.Substring(1) + "Units";

                PropertyInfo pi = this.GetType().GetProperty(methodName,BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                List<string> knownUnits = (List<string>) pi.GetValue(this);

                if (knownUnits.Contains(unit)) {
                    return true;
                } else {
                    return false;
                }

            } else {
                return false;
            }
        }
        #endregion
    }
}
