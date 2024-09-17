using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using System.Security.Cryptography;
using System.Windows.Controls;
using System.Windows.Media;

namespace RVTBootcamp_Module_02
{
    [Transaction(TransactionMode.Manual)]
    public class Module02Challenge : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            UIDocument uidoc = uiapp.ActiveUIDocument;
            IList<Element> PickList = uidoc.Selection.PickElementsByRectangle("Select elements");

            /*
             * For this session's challenge, you will create an add-in to generate Revit elements from model lines. 
             * The add-in will create different Revit elements based on the line style name. 
             * Running the add-in on the file provided below will reveal a hidden message in the Level 1 floor plan view.

                The add-in should prompt you to select elements. It should then filter the elements for model curves. 
                Once you've filtered the elements, loop through them and create the following Revit elements based on the line's line style:

                A-GLAZ - Storefront wall

                A-WALL - Generic 8" wall

                M-DUCT - Default duct

                P-PIPE - Default pipe
              
             */
            // Your code goes here


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


            using (Transaction t = new Transaction(doc))
            {
                t.Start("Create Revit Elements");
              

                Level newLevel = Level.Create(doc, 0);

                for (int i = 1; i < modelCurves.Count; i++)
                {
                    Curve curveM;
                    GraphicsStyle curveMessage;


       
                        curveM = modelCurves[i].GeometryCurve;
                        curveMessage = modelCurves[i].LineStyle as GraphicsStyle;
                   

                    WallType wallType1 = GetWallTypeByName(doc, "Storefront");
                    WallType wallType2 = GetWallTypeByName(doc, "Generic - 8\"");

                    MEPSystemType ductSysytemType = GetMEPSystemTypeByName(doc, "Exhaust Air");
                    DuctType duck_Mduck = GetDuctTypeByName(doc, "Default");

                    MEPSystemType pipeSysytemType = GetMEPSystemTypeByName(doc, "Domestic Cold Water");
                    PipeType pipe_Ppipe = GetPipeTypeByName(doc, "Default");




                    switch (curveMessage.Name)
                    {

                        case "A-GLAZ":
                            Wall AGlaze = Wall.Create(doc, curveM, wallType1.Id, newLevel.Id, 5, 0, false, false);
                            break;
                        case "A-WALL":
                            Wall AWall = Wall.Create(doc, curveM, wallType2.Id, newLevel.Id, 5, 0, false, false);
                            break;
                        case "M-DUCT":
                            Duct MDuct = Duct.Create(doc, ductSysytemType.Id, duck_Mduck.Id, newLevel.Id, curveM.GetEndPoint(0), curveM.GetEndPoint(1));
                            break;
                        case "P-PIPE":
                            Pipe PPipe = Pipe.Create(doc, pipeSysytemType.Id, pipe_Ppipe.Id, newLevel.Id, curveM.GetEndPoint(0), curveM.GetEndPoint(1));
                            break;
                        default:
                            break;

                    }
                }

                t.Commit();
            }


            return Result.Succeeded;
        }

        private MEPSystemType GetMEPSystemTypeByName(Document doc, string typeName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(MEPSystemType));

            foreach (MEPSystemType type in collector)
            {
                if (type.Name == typeName)
                    return type;
            }
            return null;
        }

        private PipeType GetPipeTypeByName(Document doc, string typeName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(PipeType));

            foreach (PipeType curType in collector)
            {
                if (curType.Name == typeName)
                {
                    return curType;
                }
            }
            return null;
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

        internal DuctType GetDuctTypeByName(Document doc, string typeName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(DuctType));

            foreach (DuctType curType in collector)
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
