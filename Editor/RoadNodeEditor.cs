using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(RoadNodeSystem))]
public class RoadNodeEditor : Editor
{
    private bool bAreasArrayExpanded;
    private string AreaDetailsStr = "Show Areas";
    private bool testButtonPressed = false;

    void FocusCameraOnPosition(Vector3 posToFocusOn, bool reselectRoadNodeSystem = false)
    {
        //TODO: find a way of focusing the camera in a 'cleaner' way

        //hack to focus camera on an object
        //1. create temp game object
        GameObject temp = new GameObject();
        //2. set object to the desired position
        temp.transform.position = posToFocusOn;
        //3. focus camera on the object 
        Selection.activeGameObject = temp;
        SceneView.FrameLastActiveSceneView();
        //4.destroy the temp game object
        Object.DestroyImmediate(temp);

        //re-select road node system object? (we selected temp object so we are loosing focus on the road node system object)
        if (reselectRoadNodeSystem)
        {
            Selection.activeGameObject = ((RoadNodeSystem)target).gameObject;
        }
    }

    void GuiLine(int i_height = 1, int leftOffset = 0)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(leftOffset);
        Rect rect = EditorGUILayout.GetControlRect(false, i_height);

        rect.height = i_height;

        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        GUILayout.EndHorizontal();
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        //Store reference to the Road Node System
        RoadNodeSystem RNS = (RoadNodeSystem)target;

        // DEBUG TOOLS
        GUILayout.Label("DEBUG");

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Debug Populate Areas"))
        {
            RNS.Debug_PopulateAreas();
        }

        if (GUILayout.Button("Debug Add New Street"))
        {
            RNS.DebugAddNewStreet(ref RNS.nodeSystem.nodeSystemOptions.areas[1].roads);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Debug Clear All"))
        {
            RNS.ClearAreas();
            testButtonPressed = true;
        }

        if (GUILayout.Button("   Fix Node IDs   "))
        {
            RNS.FixNodeIDs();
        }
        GUILayout.EndHorizontal();

        // END DEBUG TOOLS
        GUILayout.Label("Road Node System Tool");

        //Debug Arrow Colour Picker
        RNS.arrowGizmoColor = EditorGUILayout.ColorField("Direction Arrow Color", RNS.arrowGizmoColor);

        //Debug Arrow Length
        RNS.arrowGizmoMult = EditorGUILayout.Slider("Direction Arrow Size", RNS.arrowGizmoMult, 0.1f, 5.0f);

        //Node Size
        RNS.nodeGizmoRadius = EditorGUILayout.Slider("Node Gizmo Radius", RNS.nodeGizmoRadius, 0.1f, 3.0f);

        //Node Default Spacing
        GUILayout.BeginHorizontal();
        GUILayout.Label("Default Node Spacing: ");
        RNS.nodeSpacing = EditorGUILayout.FloatField(RNS.nodeSpacing);
        GUILayout.EndHorizontal();

        RNS.bDrawNodeIDs = GUILayout.Toggle(RNS.bDrawNodeIDs, "Draw Node IDs");
        RNS.logErrorOnNegativeConnections = GUILayout.Toggle(RNS.logErrorOnNegativeConnections, "Log Error On Negative Connections");

        // Make a drop-down for areas
        bAreasArrayExpanded = EditorGUILayout.Foldout(bAreasArrayExpanded, AreaDetailsStr);


        // Load areas to array
        SerializedProperty arrayPropAreas = serializedObject.FindProperty("nodeSystem.nodeSystemOptions.areas");

        //If drop-down is pressed show areas
        if (bAreasArrayExpanded && !testButtonPressed)
        {

            if (GUILayout.Button("Add New Area"))
            {
                RNS.nodeSystem.AddNewArea(ref RNS.nodeSystem.nodeSystemOptions.areas);
                //int lastElement = RNS.nodeSystem.nodeSystemOptions.areas.Length;
                //RNS.nodeSystem.AddNewRoad(ref RNS.nodeSystem.nodeSystemOptions.areas[lastElement - 1].roads);
                //arrayPropAreas = null;
            }


            EditorGUI.indentLevel = 1;

            //Load all areas to an array of type area 
            RoadNodeSystem.NodeSystem.AreaSystem[] areaArr = RNS.nodeSystem.nodeSystemOptions.areas; // All areas

            if (arrayPropAreas != null && areaArr != null && arrayPropAreas.arraySize > 0 && areaArr.Length > 0)
                for (int i = 0; i < arrayPropAreas.arraySize; ++i)
                {
                    AreaDetailsStr = "Hide Areas";

                    //Make a drop-down for each area
                    areaArr[i].ShowDetails = EditorGUILayout.Foldout(areaArr[i].ShowDetails, "Area_" + i + " (" + areaArr[i].AreaName + ")");

                    if (areaArr[i].ShowDetails && areaArr.Length > 0)
                    {
                        SerializedProperty arr = arrayPropAreas.GetArrayElementAtIndex(i);

                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Area Name");
                        areaArr[i].AreaName = GUILayout.TextField(areaArr[i].AreaName);
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Space(5);
                        if (GUILayout.Button("Add New Street"))
                        {
                            RNS.nodeSystem.AddNewRoad(ref RNS.nodeSystem.nodeSystemOptions.areas[i].roads, i);
                        }
                        GUILayout.EndHorizontal();

                        if (areaArr != null && areaArr.Length != 0 && arr != null && areaArr[i].roads != null && areaArr[i].roads.Length > 0)
                        {
                            for (int b = 0; b < areaArr[i].roads.Length; ++b)
                            {
                                EditorGUI.indentLevel = 2;

                                if (areaArr[i].roads[b].ShowRoadDetails)
                                    GUILayout.BeginHorizontal();

                                //STREET LEVEL
                                // SerializedProperty arrayStreetProp = arr.FindPropertyRelative("roads");
                                //SerializedProperty arrStr = arrayStreetProp.GetArrayElementAtIndex(i);
                                //EditorGUILayout.PropertyField(arrSrr, new GUIContent("Street_" + b + "_" + areaArr[i].roads[b].StreetName));
                                string streetName = "";
                                if (!(areaArr[i].roads[b].ShowRoadDetails))
                                    streetName = " (" + areaArr[i].roads[b].StreetName + ")";

                                GUI.contentColor = Color.yellow;
                                areaArr[i].roads[b].ShowRoadDetails = EditorGUILayout.Foldout(areaArr[i].roads[b].ShowRoadDetails, "Street_" + b + streetName);
                                GUI.contentColor = Color.white;

                                EditorGUI.indentLevel = 1;

                                //Road/Street nodes
                                if (areaArr[i].roads[b].ShowRoadDetails)
                                {
                                    areaArr[i].roads[b].StreetName = GUILayout.TextField(areaArr[i].roads[b].StreetName);
                                    GUILayout.EndHorizontal();

                                    GUILayout.BeginHorizontal();
                                    GUILayout.Space(30);
                                    if (GUILayout.Button("Add New Node"))
                                    {
                                        RNS.nodeSystem.AddNewNode(ref areaArr[i].roads[b].nodes, i, b, RNS.nodeSpacing);
                                    }
                                    if (GUILayout.Button("Reposition Nodes"))
                                    {
                                        RNS.nodeSystem.ResetPostionOfTheChildrenNodes(ref areaArr[i].roads[b].nodes);
                                    }
                                    GUILayout.EndHorizontal();
                                    GUILayout.BeginHorizontal();
                                    GUILayout.Space(30);
                                    if (GUILayout.Button("Remove Last Node"))
                                    {
                                        RNS.nodeSystem.RemoveLastNode(ref areaArr[i].roads[b].nodes);
                                    }
                                    if (GUILayout.Button("   Reverse Direction   "))
                                    {
                                        RNS.nodeSystem.ReverseDirection(ref areaArr[i].roads[b].nodes);
                                    }
                                    GUILayout.EndHorizontal();
                                    GUILayout.BeginHorizontal();
                                    GUILayout.Space(30);
                                    if (GUILayout.Button("   Reset All Connections   "))
                                    {
                                        RNS.nodeSystem.ResetAllConnectionsInAGivenStreet(ref areaArr[i].roads[b].nodes, i, b, true);
                                    }
                                    GUILayout.EndHorizontal();

                                    EditorGUI.indentLevel = 3;
                                    areaArr[i].roads[b].ShowNodeDetails = EditorGUILayout.Foldout(areaArr[i].roads[b].ShowNodeDetails, "Road Nodes (size = " + areaArr[i].roads[b].nodes.Length + ")");

                                    if (areaArr[i].roads[b].ShowNodeDetails)
                                    {
                                        //EditorGUILayout.PropertyField(arrStr.FindPropertyRelative("nodes"), new GUIContent("<>"));

                                        for (int c = 0; c < areaArr[i].roads[b].nodes.Length; ++c)
                                        {
                                            GUI.contentColor = Color.cyan;
                                            ref var node = ref areaArr[i].roads[b].nodes[c];

                                            GUILayout.BeginHorizontal();
                                            EditorGUILayout.LabelField("Node " + c);

                                            if (GUILayout.Button("Focus Camera"))
                                            {
                                                FocusCameraOnPosition(node.Position, true);
                                            }
                                            GUILayout.EndHorizontal();
                                            GUILayout.Space(5);

                                            GUI.contentColor = Color.white;
                                            node.Position = EditorGUILayout.Vector3Field("Position", node.Position);
                                            GUILayout.Space(5);

                                            //Draw connections
                                            {
                                                // Make a drop-down for connections
                                                GUILayout.BeginHorizontal();
                                                GUILayout.Space(10);
                                                string connectionsText = node.bExpandConnections ? "hide connections" : "show connections";
                                                connectionsText += node.mConnections != null ? " (" + node.mConnections.Count.ToString() +")" : "null";
                                                //node.bExpandConnections = EditorGUILayout.Foldout(node.bExpandConnections, node.bExpandConnections ? "hide connections" : "show connections");
                                                node.bExpandConnections = EditorGUILayout.Foldout(node.bExpandConnections, connectionsText);
                                                GUILayout.EndHorizontal();

                                                // Load connections to array
                                                if (node.bExpandConnections && node.mConnections != null)
                                                {
                                                    GUILayout.BeginHorizontal();
                                                    GUILayout.Space(45);
                                                    if (GUILayout.Button("Add New Connection"))
                                                    {
                                                        node.AddNewConnection(-1, -1, -1);
                                                    }
                                                    GUILayout.EndHorizontal();

                                                    GUILayout.BeginVertical();
                                                    for (int linkIndex = 0; linkIndex < node.mConnections.Count; linkIndex++)
                                                    {
                                                        var link = node.mConnections[linkIndex];

                                                        GUILayout.BeginHorizontal();
                                                        //GUILayout.Space(0);
                                                        GUILayout.FlexibleSpace();
                                                        
                                                        int area = node.mConnections[linkIndex].AreaID;
                                                        int road = node.mConnections[linkIndex].RoadID;
                                                        int id   = node.mConnections[linkIndex].NodeID;

                                                        GUILayout.Space(-60);
                                                        GUI.contentColor = link.bAreaIDOutOfRange ? Color.red : Color.white;
                                                        EditorGUILayout.LabelField("Area ID");
                                                        GUILayout.Space(-130);
                                                        area = EditorGUILayout.IntField(area);

                                                        GUILayout.Space(-45);
                                                        GUI.contentColor = link.bRoadIDOutOfRange ? Color.red : Color.white;
                                                        EditorGUILayout.LabelField("Street ID");
                                                        GUILayout.Space(-120);
                                                        road = EditorGUILayout.IntField(road);
 
                                                        GUILayout.Space(-40);
                                                        GUI.contentColor = link.bNodeIDOutOfRange ? Color.red : Color.white;
                                                        EditorGUILayout.LabelField("Node ID");
                                                        GUILayout.Space(-125);
                                                        id   = EditorGUILayout.IntField(id);
                                                        GUI.contentColor = Color.white;

                                                        node.ModifyConnection(linkIndex, area, road, id);
                                                        if (GUILayout.Button("Delete"))
                                                        {
                                                            node.RemoveConnection(linkIndex);
                                                        }
                                                        GUILayout.EndHorizontal();
                                                    }
                                                    //GUI.contentColor = Color.white;
                                                    GUILayout.EndVertical();     
                                                }
                                            }

                                            //Separate nodes
                                            GUILayout.Space(5);
                                            GuiLine(1, 45);
                                            GUILayout.Space(10);
                                        }
                                    }

                                    EditorGUI.indentLevel = 1;
                                }
                            }//end for loop
                        }//end if
                    }
                }

        }

        else
            AreaDetailsStr = "Show Areas";


        testButtonPressed = false;
    }//OnInspectorGUI() end
}
