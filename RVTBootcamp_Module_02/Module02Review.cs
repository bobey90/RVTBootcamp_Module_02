using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using System.Collections.Generic;

namespace RVTBootcamp_Module_02
{
    [Transaction(TransactionMode.Manual)]
    public class Module02Review : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;
            
            // Your code goes here

            //1. prompt user to select elements
            TaskDialog.Show("Select lines", "Select some lines to convert to Revit elements");
            IList<Element> pickList = uidoc.Selection.PickElementsByRectangle("Select some Elements").ToList();

            //2. Filter selected elements

            List<CurveElement> filteredList = new List<CurveElement>();
            foreach (Element element in pickList)
            {
                if(element is CurveElement curve)               
                    filteredList.Add(curve);               
            }

            //TaskDialog.Show("Curves ", "You selected " + filteredList.Count + " curves");
            TaskDialog.Show("Curves ", $"You selected  {filteredList.Count} curves");

            //3. get the level and types

            Parameter levelParam = doc.ActiveView.LookupParameter("Associated Level");
            //Level currentLevel = doc.GetElement(levelParam.AsElementId()) as Level;
            Level currentLevel = GetLevelByName(doc, levelParam.AsString());


            //4. get types
            WallType wt1 = GetWallTypeByName(doc, "Storefront");
            WallType wt2 = GetWallTypeByName(doc, "Generic - 8\"");

            MEPSystemType ductSystemType = GetMEPSystemTypeByName(doc, "Supply Air");
            DuctType ductType = GetDuctTypeByName(doc, "Default");

            MEPSystemType pipeSystemType = GetMEPSystemTypeByName(doc, "Domestic Hot Water");
            PipeType pipeType = GetPipeTypeByName(doc, "Default");

            List<ElementId> linesToHide = new List<ElementId>();

            //5. loop through the curves and create elements

            using (Transaction t = new Transaction(doc, "Create Revit Elemtns"))
            {
                t.Start();

                foreach(CurveElement curCurve in filteredList)
                {
                    //6. Get Graphicstyle and curve from the curve element
                    Curve elemCurve = curCurve.GeometryCurve;
                    GraphicsStyle curStyle = curCurve.LineStyle as GraphicsStyle;

                    //7. skip arcs
                    if (elemCurve.IsBound == false)
                    {
                        linesToHide.Add(curCurve.Id);
                        continue;
                    }
                    //8.b



                    //8. Switch Statement to create elements

                    

                    switch (curStyle.Name)
                    {

                        case "A-GLAZ":
                            //create wall
                            Wall curWall = Wall.Create(doc, elemCurve, wt1.Id, currentLevel.Id, 20, 0, false, false);
                            break;

                        case "A-WALL":
                            //create wall
                            Wall curWall2 = Wall.Create(doc, elemCurve, wt2.Id, currentLevel.Id, 20, 0, false, false);
                            break;

                        case "M-DUCT":
                            //create wall
                            Duct curDuct = Duct.Create(doc, ductSystemType.Id, ductType.Id, currentLevel.Id, elemCurve.GetEndPoint(0), elemCurve.GetEndPoint(1));
                            break;

                        case "P-PIPE":
                            //create wall
                            Pipe curPipe = Pipe.Create(doc, pipeSystemType.Id, pipeType.Id, currentLevel.Id, elemCurve.GetEndPoint(0), elemCurve.GetEndPoint(1));
                            break;

                        default:
                            //hide the line
                            linesToHide.Add(curCurve.Id);
                            break;
                    }


                }

                doc.ActiveView.HideElements(linesToHide);




                t.Commit();
            }



            return Result.Succeeded;
        }

        private PipeType GetPipeTypeByName(Document doc, string value)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(PipeType));

            foreach (PipeType type in collector)
            {
                if (type.Name == value)
                    return type;
            }
            return null;
        }

        private DuctType GetDuctTypeByName(Document doc, string value)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(DuctType));

            foreach (DuctType type in collector)
            {
                if (type.Name == value)
                    return type;
            }
            return null;
        }

        private MEPSystemType GetMEPSystemTypeByName(Document doc, string value)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(MEPSystemType));

            foreach (MEPSystemType type in collector)
            {
                if (type.Name == value)
                    return type;
            }
            return null;
        }
        private WallType GetWallTypeByName(Document doc, string value)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(WallType));

            foreach (WallType type in collector)
            {
                if (type.Name == value)
                    return type;
            }
            return null;
        }

        private Level GetLevelByName(Document doc, string value)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(Level));

            foreach(Level level in collector)
            {
                if (level.Name == value)
                    return level;
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
