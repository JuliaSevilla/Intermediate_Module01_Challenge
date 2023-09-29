#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Resources;

#endregion

namespace Intermediate_Module01_Challenge
{
    [Transaction(TransactionMode.Manual)]
    public class Command1 : IExternalCommand
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
                t.Start("Create schedule");
                // 01. Filter by department
                FilteredElementCollector roomCollector = new FilteredElementCollector(doc);
                roomCollector.OfCategory(BuiltInCategory.OST_Rooms);
                roomCollector.WhereElementIsNotElementType();

                List<string> deparmentsList = new List<string> ();

                foreach (Room room in roomCollector)
                {
                    string department = GetParameterValueAsString(room, "Department");
                    deparmentsList.Add(department);
                }

                List<string> uniqueDepartmentsList = deparmentsList.Distinct().ToList();
                uniqueDepartmentsList.Sort();


                foreach (string departmentValue in uniqueDepartmentsList)
                {
                    //02a. Get parameters for fields
                    ElementId catId = new ElementId(BuiltInCategory.OST_Rooms);
                    ViewSchedule newSchedule = ViewSchedule.CreateSchedule(doc, catId);
                    newSchedule.Name = "Dept - " + departmentValue;
                    

                    Element roomInst = roomCollector.FirstElement();
                    Parameter roomLevelParam = roomInst.LookupParameter("Level");
                    Parameter roomNumberParam = roomInst.get_Parameter(BuiltInParameter.ROOM_NUMBER);
                    Parameter roomNameParam = roomInst.get_Parameter(BuiltInParameter.ROOM_NAME);
                    Parameter roomDeptParam = roomInst.get_Parameter(BuiltInParameter.ROOM_DEPARTMENT);
                    Parameter roomCommentsParam = roomInst.LookupParameter("Comments");
                    Parameter roomAreaParam = roomInst.get_Parameter(BuiltInParameter.ROOM_AREA);

                    // 02b. Create fields
                    ScheduleField roomNumberField = newSchedule.Definition.AddField(ScheduleFieldType.Instance, roomNumberParam.Id);
                    ScheduleField roomNameField = newSchedule.Definition.AddField(ScheduleFieldType.Instance, roomNameParam.Id);
                    ScheduleField roomDeptField = newSchedule.Definition.AddField(ScheduleFieldType.Instance, roomDeptParam.Id);
                    ScheduleField roomCommentsField = newSchedule.Definition.AddField(ScheduleFieldType.Instance, roomCommentsParam.Id);
                    ScheduleField roomLevelField = newSchedule.Definition.AddField(ScheduleFieldType.Instance, roomLevelParam.Id);
                    ScheduleField roomAreaField = newSchedule.Definition.AddField(ScheduleFieldType.ViewBased, roomAreaParam.Id);

                    roomLevelField.IsHidden = true;

                    // Filter by department
                    
                    ScheduleFilter depFilter = new ScheduleFilter(roomDeptField.FieldId, ScheduleFilterType.Equal, departmentValue);
                    newSchedule.Definition.AddFilter(depFilter);

                    //03c. Group by level
                    ScheduleSortGroupField typeSort = new ScheduleSortGroupField(roomLevelField.FieldId);
                    typeSort.ShowHeader = true;
                    typeSort.ShowBlankLine = true;
                    newSchedule.Definition.AddSortGroupField(typeSort);

                    //03d. Sort by room name
                    ScheduleSortGroupField markSort = new ScheduleSortGroupField(roomNameField.FieldId);
                    newSchedule.Definition.AddSortGroupField(markSort);

                    //03e. Show total Area for each level group
                    roomAreaField.DisplayType = ScheduleFieldDisplayType.Totals;

                    //03f. Totals for the departments
                    newSchedule.Definition.ShowGrandTotal = true;
                    newSchedule.Definition.ShowGrandTotalTitle = true; 
                    newSchedule.Definition.ShowGrandTotalCount = true;

                }

                t.Commit();
            }
            return Result.Succeeded;
        }

        
        private static string GetParameterValueAsString(Room room, string paramName)
        {
            IList<Parameter> paramList = room.GetParameters(paramName);
            Parameter myParam = paramList.First();

            return myParam.AsString();
        }

        
        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand1";
            string buttonTitle = "Button 1";

            ButtonDataClass myButtonData1 = new ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This is a tooltip for Button 1");

            return myButtonData1.Data;
        }
    }
}
