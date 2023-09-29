#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

#endregion

namespace Intermediate_Module01_Challenge
{
    [Transaction(TransactionMode.Manual)]
    public class Command2 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            // Your code goes here
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Create all departments schedule");
                
                // 01.Create Schedule
                ElementId catId = new ElementId(BuiltInCategory.OST_Rooms);
                ViewSchedule newSchedule = ViewSchedule.CreateSchedule(doc, catId);
                newSchedule.Name = "All Departments";

                //02a. Get parameters for fields
                FilteredElementCollector roomCollector = new FilteredElementCollector(doc);
                roomCollector.OfCategory(BuiltInCategory.OST_Rooms);
                roomCollector.WhereElementIsNotElementType();

                Element roomInst = roomCollector.FirstElement();
                Parameter roomAreaParam = roomInst.get_Parameter(BuiltInParameter.ROOM_AREA);
                Parameter roomDeptParam = roomInst.get_Parameter(BuiltInParameter.ROOM_DEPARTMENT);

                //02b.Create fields

                ScheduleField roomDeptField = newSchedule.Definition.AddField(ScheduleFieldType.Instance, roomDeptParam.Id);
                ScheduleField roomAreaField = newSchedule.Definition.AddField(ScheduleFieldType.ViewBased, roomAreaParam.Id);

                //03d. Sorted by department
                ScheduleSortGroupField markSort = new ScheduleSortGroupField(roomDeptField.FieldId);
                newSchedule.Definition.AddSortGroupField(markSort);

                //03e. Show total Area for each level group
                roomAreaField.DisplayType = ScheduleFieldDisplayType.Totals;

                //03d. Schedule not itemized each room 
                newSchedule.Definition.IsItemized = true;

                //03f. Totals for the departments
                newSchedule.Definition.ShowGrandTotal = true;
                newSchedule.Definition.ShowGrandTotalTitle = true;

                t.Commit();
            }

            return Result.Succeeded;
        }
        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand2";
            string buttonTitle = "Button 2";

            ButtonDataClass myButtonData1 = new ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This is a tooltip for Button 2");

            return myButtonData1.Data;
        }
    }
}
