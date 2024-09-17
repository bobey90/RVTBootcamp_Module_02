using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;

namespace RVTBootcamp_Module_02
{
    [Transaction(TransactionMode.Manual)]
    public class Module02 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

           // Your code goes here
           
            

            // 1. pick elemets and filter them into list
            UIDocument uidoc = uiapp.ActiveUIDocument;
            IList<Element> PickList = uidoc.Selection.PickElementsByRectangle("Select elements");

            TaskDialog.Show("Test", "I selected " + PickList.Count.ToString() + " elements");


            //2a. filter selected elements for curves

            List<CurveElement> allCurves = new List<CurveElement>();
            foreach (Element elem in PickList)
            {
                if (elem is CurveElement)
                {
                    allCurves.Add(elem as CurveElement);
                }
            }

            //2b. filter selected elements for model curves
            List<CurveElement> modelCurves = new List<CurveElement>();
            foreach (Element elem in PickList)
            {
                if (elem is CurveElement)
                {
                    CurveElement curveElem = elem as CurveElement;
                    //CurveElement curveElem = (CurveElement) elem;
                    if (curveElem.CurveElementType == CurveElementType.ModelCurve)
                    {
                        modelCurves.Add(curveElem);
                    }
                }
            }

            //3. curve data

            foreach (CurveElement currentCurve in modelCurves)
            {
                Curve curve = currentCurve.GeometryCurve;
                XYZ startPoint = curve.GetEndPoint(0);
                XYZ endPoint = curve.GetEndPoint(1);

                GraphicsStyle curStyle = currentCurve.LineStyle as GraphicsStyle;

                Debug.Print(curStyle.Name);
            }

            //5. create transaction with using statment
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Create Revit Elements");

                // 4. create wall

                Level newLevel = Level.Create(doc, 20);
                Curve curCurve1 = modelCurves[0].GeometryCurve;

                Wall.Create(doc, curCurve1, newLevel.Id, false);

                FilteredElementCollector wallTypes = new FilteredElementCollector(doc);
                wallTypes.OfClass(typeof(WallType));

                Curve curCurve2 = modelCurves[1].GeometryCurve;
                //WallType myWallType = GetWallTypeByName(doc, "Exterior - Brick on CMU");
                Wall.Create(doc, curCurve2, wallTypes.FirstElementId(), newLevel.Id, 20, 0, false, false);

                //6. get system types
                FilteredElementCollector systemCollector = new FilteredElementCollector(doc);
                systemCollector.OfClass(typeof(MEPSystemType));

                //7.get duct system type
                MEPSystemType ductSystemType = null;
                foreach (MEPSystemType curType in systemCollector)
                {
                    if (curType.Name == "Supply Air")
                    {
                        ductSystemType = curType;
                        break;
                    }
                }

                //8. get duct type
                FilteredElementCollector collector1 = new FilteredElementCollector(doc);
                collector1.OfClass(typeof(DuctType));

                //9. create duct
                Curve curCurve3 = modelCurves[2].GeometryCurve;
                Duct newDuct = Duct.Create(doc,
                    ductSystemType.Id, collector1.FirstElementId(), newLevel.Id,
                    curCurve3.GetEndPoint(0), curCurve3.GetEndPoint(1));

                //10.get duct system type
                MEPSystemType pipeSystemType = null;
                foreach (MEPSystemType curType in systemCollector)
                {
                    if (curType.Name == "Domestic Hot Water")
                    {
                        pipeSystemType = curType;
                        break;
                    }
                }

                //            //11. get pipe type
                //            FilteredElementCollector collector2 = new FilteredElementCollector(doc);
                //            collector2.OfClass(typeof(PipeType));

                //            //10. create pipe
                //            Curve curCurve4 = modelCurves[3].GeometryCurve;
                //            Pipe.Create(doc,
                //                pipeSystemType.Id, collector2.FirstElementId(), newLevel.Id,
                //                curCurve4.GetEndPoint(0), curCurve4.GetEndPoint(1));

                //            //13.use our new methods
                //            string teststring = MyFirstMethod();
                //            MySecondMethod();
                //            string testString2 = MyThirdMethod("Hello World");
                //            TaskDialog.Show(teststring, testString2);

                //            //15. switch statement
                //            int numberValue = 5;
                //            string numAsString = "";

                //            switch (numberValue)
                //            {
                //                case 1:
                //                    numAsString = "One";
                //                    break;
                //                case 2:
                //                    numAsString = "Two";
                //                    break;
                //                case 3:
                //                    numAsString = "Three";
                //                    break;
                //                case 4:
                //                    numAsString = "Four";
                //                    break;
                //                case 5:
                //                    numAsString = "Five;";
                //                    break;
                //                default:
                //                    numAsString = "Zero";
                //                    break;
                //            }

                //            //16. advanced switch statements
                //            Curve curve5 = modelCurves[1].GeometryCurve;
                //            GraphicsStyle curve5GS = modelCurves[1].LineStyle as GraphicsStyle;
                //            WallType wallType1 = GetWallTypeByName(doc, "Storefront");
                //            WallType wallType2 = GetWallTypeByName(doc, "Exterior - Brick on CMU");

                //    switch (curve5GS.Name)
                //    {

                //        case "<Thin Line>":
                //            Wall.Create(doc, curve5, wallType1.Id, newLevel.Id, 20, 0, false, false);
                //            break;
                //        case "<Wide Lines>":
                //            Wall.Create(doc, curve5, wallType2.Id, newLevel.Id, 20, 0, false, false);
                //            break;
                //        default:
                //            Wall.Create(doc, curve5, newLevel.Id, false);
                //            break;
                //    }


                t.Commit();
            }

            //make a change in the revit model
            return Result.Succeeded;
        }

        internal string MyFirstMethod()
        {
            return "This is my first method";
        }
        internal void MySecondMethod()
        {
            Debug.Print("This is my second method");
        }
        internal string MyThirdMethod(string input)
        {
            return "This is my thrid method: " + input;
        }

        internal WallType GetWallTypeByName(Document doc, string typeName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(WallType));

            foreach (WallType curType in collector)
            {
                if (curType.Name == typeName)
                {
                    return curType;
                }
            }
            return null;
        }
        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand1";
            string buttonTitle = "Button 1";
            string? methodBase = MethodBase.GetCurrentMethod().DeclaringType?.FullName;

            if (methodBase == null)
            {
                throw new InvalidOperationException("MethodBase.GetCurrentMethod().DeclaringType?.FullName is null");
            }
            else
            {
                Common.ButtonDataClass myButtonData1 = new Common.ButtonDataClass(
                    buttonInternalName,
                    buttonTitle,
                    methodBase,
                    Properties.Resources.Blue_32,
                    Properties.Resources.Blue_16,
                    "This is a tooltip for Button 1");

                return myButtonData1.Data;
            }
        }
    }

}
