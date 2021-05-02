using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

public class RoadNodeSystem : MonoBehaviour
{
    #region variables
    public float nodeGizmoRadius = 0.5f;
    public float nodeSpacing = 5.0f;
    [Range(0.0f, 3.0f)]
    public float arrowGizmoMult = 0.5f;
    public Color arrowGizmoColor = Color.green;
    public NodeSystem nodeSystem;
    public bool logErrorOnNegativeConnections = true;
    public bool bDrawNodeIDs = true;
    #endregion

    private void Awake()
    {
        Debug.Log("Editor causes this Awake");
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Editor causes this Start");
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Editor causes this Update");
    }

    private void OnDrawGizmos()
    {
        DrawNodes();
    }

    private bool IntInRangeCheck(int num, int min, int max)
    {
        if (num >= min && num <= max)
            return true;

        return false;
    }

    Vector3[] GetDebugArrowBasedonDirection(Vector3 start, Vector3 end)
    {
        // Element 0 - Left Arrow, Element 1 - Right Arrow
        Vector3[] arrows = { new Vector2(0.0f, 0.0f), new Vector2(0.0f, 0.0f) };

        //Vector3 axisForRotation = Vector3.Cross(start, end);

        Vector3 direction = new Vector3(-0.2f, 0f, 0.2f);

        //arrows[0] = end - start - Vector3.left - Vector3.forward;
        arrows[0] = end - start - direction * Vector3.Magnitude(end - start);
        arrows[0] = end - arrows[0].normalized * arrowGizmoMult;

        //arrows[1] = end - start + Vector3.left + Vector3.forward;
        arrows[1] = end - start + direction * Vector3.Magnitude(end - start);
        arrows[1] = end - arrows[1].normalized * arrowGizmoMult;

        return arrows;
    }

    [System.Serializable]
    public struct NodeSystem
    {
        // NodeSystemOptions>AreaSystem>RoadSystem>Node

        [System.Serializable]
        public struct NodeConnection
        {
            public int AreaID, RoadID, NodeID;
            public bool bAreaIDOutOfRange, bRoadIDOutOfRange, bNodeIDOutOfRange;

            public void SetValues(int areaID, int roadID, int nodeID)
            {
                AreaID = areaID;
                RoadID = roadID;
                NodeID = nodeID;
            }

            public void SetOutOfRangeValues(bool area, bool road, bool node)
            {
                bAreaIDOutOfRange = area;
                bRoadIDOutOfRange = road;
                bNodeIDOutOfRange = node;
            }

            public override string ToString()
            {
                return "A[" + AreaID + "] S[" + RoadID + "] N[" + NodeID + "]";
            }
        }

        [System.Serializable]
        public struct Node
        {
            public Vector3 Position;
            public bool bMainNode;
            public NodeConnection ID;
            public List<NodeConnection> mConnections;
            public bool bExpandConnections;

            public void AddNewConnection(int areaID, int roadID, int nodeID)
            {
                if(mConnections != null)
                {
                    var tempConnection = new NodeConnection();
                    tempConnection.SetValues(areaID, roadID, nodeID);
                    tempConnection.SetOutOfRangeValues(true, true, true);
                    mConnections.Add(tempConnection);
                }
            }

            public void AddNewConnection(ref Node fromNode)
            {
                AddNewConnection(fromNode.ID.AreaID, fromNode.ID.RoadID, fromNode.ID.NodeID);
            }

            public void SetConnectionOutOfRange(int index, bool isAreaOutOfRange, bool isRoadOutOfRange, bool isNodeOutOfRange)
            {
                var newConnection = new NodeConnection();
                var oldConnection = mConnections[index];
                newConnection.SetValues(oldConnection.AreaID, oldConnection.RoadID, oldConnection.NodeID);
                newConnection.SetOutOfRangeValues(isAreaOutOfRange, isRoadOutOfRange, isNodeOutOfRange);
                mConnections[index] = newConnection;
            }

            //Modify a connection
            public void ModifyConnection(int index, int newAreaID, int newRoadID, int newNodeID)
            {
                var newConnection = new NodeConnection();
                var oldConnection = mConnections[index];
                newConnection.SetValues(newAreaID, newRoadID, newNodeID);
                newConnection.SetOutOfRangeValues(oldConnection.bAreaIDOutOfRange, oldConnection.bRoadIDOutOfRange, oldConnection.bNodeIDOutOfRange);
                mConnections[index] = newConnection;
            }

            public void RemoveConnection(int connectionIndex)
            {
                //check if we got a valid index
                if (connectionIndex > -1 && connectionIndex < mConnections.Count)
                {
                    mConnections.RemoveAt(connectionIndex);
                }
            }
        }

        [System.Serializable]
        public struct RoadSystem
        {
            [SerializeField] public bool ShowRoadDetails;
            [SerializeField] public bool ShowNodeDetails;
            [SerializeField] public string StreetName;
            [SerializeField] public Node[] nodes;
        }

        [System.Serializable]
        public struct AreaSystem
        {
            [SerializeField] public bool ShowDetails;
            [SerializeField] public string AreaName;
            [SerializeField] public RoadSystem[] roads;
        }

        [System.Serializable]
        public struct NodeSystemOption
        {
            [SerializeField] public AreaSystem[] areas;
        }



        public NodeSystemOption nodeSystemOptions;


        #region Editor
        #region Creation
        public void AddNewNode(ref Node[] refToNodeArray, int areaID, int roadID, float defaultNodeSpacing)
        {
            int NodeArrSize = refToNodeArray.Length;

            //Copy and expand the area array
            NodeSystem.Node[] tempArr = new NodeSystem.Node[NodeArrSize + 1];
            refToNodeArray.CopyTo(tempArr, 0);
            refToNodeArray = tempArr;

            //reference to the new node being added (current node)
            ref var cNode = ref refToNodeArray[NodeArrSize];

            //Setup ID & connections
            cNode.ID = new NodeConnection();
            cNode.ID.SetValues(areaID, roadID, NodeArrSize);
            cNode.bExpandConnections = false;

            cNode.mConnections = new List<NodeConnection>();
            //

            if (NodeArrSize > 0)
            {
                //move forward from the previous node
                refToNodeArray[NodeArrSize].Position = refToNodeArray[NodeArrSize - 1].Position + Vector3.forward * defaultNodeSpacing;

                //make a new connection with the previous node
                //but first check if the value was initialized correctly
                if (refToNodeArray[NodeArrSize - 1].mConnections == null)
                    refToNodeArray[NodeArrSize - 1].mConnections = new List<NodeConnection>();

                refToNodeArray[NodeArrSize - 1].mConnections.Add(cNode.ID);
            }

            //First node added. Make it the main node.
            else
            {
                refToNodeArray[0].bMainNode = true;
            }
        }

        public void AddNewRoad(ref RoadSystem[] refToRoadArray, int AreaID)
        {
            int RoadArrSize = refToRoadArray.Length;

            //Copy and expand the area array
            NodeSystem.RoadSystem[] tempArr = new NodeSystem.RoadSystem[RoadArrSize + 1];
            refToRoadArray.CopyTo(tempArr, 0);
            refToRoadArray = tempArr;

            refToRoadArray[RoadArrSize].StreetName = "NewStreet";
            refToRoadArray[RoadArrSize].nodes = new NodeSystem.Node[1];
            refToRoadArray[RoadArrSize].nodes[0].bMainNode = true;
            refToRoadArray[RoadArrSize].nodes[0].ID.SetValues(AreaID, RoadArrSize, 0);
        }

        public void AddNewArea(ref AreaSystem[] refToAreaArray)
        {
            int AreaArrSize = refToAreaArray.Length;

            //Copy and expand the area array
            NodeSystem.AreaSystem[] tempArr = new NodeSystem.AreaSystem[AreaArrSize + 1];

            if (AreaArrSize > 0)
                refToAreaArray.CopyTo(tempArr, 0); // copy old values

            refToAreaArray = tempArr;

            refToAreaArray[AreaArrSize].AreaName = "NewArea";
            refToAreaArray[AreaArrSize].roads = new RoadSystem[1];
            refToAreaArray[AreaArrSize].roads[0].StreetName = "NewStreet";
            refToAreaArray[AreaArrSize].roads[0].nodes = new NodeSystem.Node[1];
            refToAreaArray[AreaArrSize].roads[0].nodes[0].bMainNode = true;
            refToAreaArray[AreaArrSize].roads[0].nodes[0].ID.SetValues(AreaArrSize, 0, 0);
        }

        public void ResetAllConnectionsInAGivenStreet(ref Node[] refToNodeArray, int AreaID, int RoadID, bool AddConnectionsBasedOnArrayPosition = true)
        {
            for(int i = 0; i < refToNodeArray.Length; i++)
            {
                ref var node = ref refToNodeArray[i];

                if (node.mConnections != null)
                {
                    node.mConnections.Clear();

                    if (AddConnectionsBasedOnArrayPosition && i > 0)
                    {
                        ref var prevNode = ref refToNodeArray[i - 1];
                        prevNode.AddNewConnection(AreaID, RoadID, i);
                    }

                    //fix ID if there is a mismatch
                    if(node.ID.NodeID != i || node.ID.RoadID != RoadID || node.ID.AreaID != AreaID)
                    {
                        node.ID.SetValues(AreaID, RoadID, i);
                    }
                }
            }
        }

        #endregion
        #region Tools
        public void ResetPostionOfTheChildrenNodes(ref Node[] refToNodeArray)
        {
            if(refToNodeArray.Length > 2)
            {
                //Vector3 distance = refToNodeArray[1].Position - refToNodeArray[0].Position;

                for (int i = 2; i < refToNodeArray.Length; ++i)
                {
                    //refToNodeArray[i].Position = refToNodeArray[0].Position + distance * i;
                    refToNodeArray[i].Position = refToNodeArray[i - 1].Position + (refToNodeArray[i - 1].Position - refToNodeArray[i - 2].Position); // position of the previous node + the distance between the previous node adn its previous node
                }

                //Debug
                Vector3 v3_distance = refToNodeArray[1].Position - refToNodeArray[0].Position;
                float f_magnitude = Vector3.Magnitude(v3_distance);

                Debug.Log("Nodes in the array repositioned successfully.\n[Difference = " + v3_distance + ", Mag (distance) = " + f_magnitude + "]");
            }
        }
        public void ReverseDirection(ref Node[] refToNodeArray)
        {
            refToNodeArray[0].bMainNode = false;
            Array.Reverse(refToNodeArray);
            refToNodeArray[0].bMainNode = true;
        }
        public void RemoveLastNode(ref Node[] refToNodeArray)
        {
            int NodeArrSize = refToNodeArray.Length;

            if (NodeArrSize > 0)
            {

                //Copy and expand the area array
                NodeSystem.Node[] tempArr = new NodeSystem.Node[NodeArrSize - 1];

                for (int i = 0; i < refToNodeArray.Length - 1; ++i) //copy all nodes except the last one
                    tempArr[i] = refToNodeArray[i];
 
                refToNodeArray = tempArr;

            }
        }

        #endregion
        #endregion

    }

    public void DrawNodes()
    {
        if (nodeSystem.nodeSystemOptions.areas.Length > 0 && nodeSystem.nodeSystemOptions.areas != null)
            foreach (var area in nodeSystem.nodeSystemOptions.areas)
            {
                if (area.roads != null && area.roads.Length > 0)
                    foreach (var road in area.roads)
                    {
                        int index = 0;

                        if (road.nodes.Length != 0)
                            for (int nodeIndex = 0; nodeIndex < road.nodes.Length; nodeIndex++)
                            {
                                ref var node = ref road.nodes[nodeIndex];
                                Gizmos.color = (node.bMainNode) ? Color.green : nodeIndex == road.nodes.Length - 1 ? Color.red : Color.blue;

                                Gizmos.DrawSphere(node.Position, nodeGizmoRadius);

                                if(bDrawNodeIDs)
                                    Handles.Label(node.Position + Vector3.up, node.ID.ToString());

                                if (node.mConnections != null && node.mConnections.Count > 0)
                                {
                                    for(int connectionIndex = 0; connectionIndex < node.mConnections.Count; connectionIndex++)
                                    {
                                        // Any area, road and node with a negative number/ID will be ignored (-1 can be set to disable a connection.
                                        // However, it's still advised to delete the unnecessary connection instead of disabling it. )

                                        var connection = node.mConnections[connectionIndex];

                                        //Set AreaID, StreetID, NodeID as out of range so we can mark them as red in the editor to help to catch any out of bounds values.
                                        //Warning: if the area is out of bounds then street and node will be also marked red.
                                        node.SetConnectionOutOfRange(connectionIndex, true, true, true);

                                        //Check if the specified area is in range
                                        int areaArrLen = nodeSystem.nodeSystemOptions.areas.Length - 1;
                                        if (IntInRangeCheck(connection.AreaID, 0, areaArrLen))
                                        {
                                            //AreaID is definitely within bounds so mark 
                                            node.SetConnectionOutOfRange(connectionIndex, false, true, true);
                                            //Check if the specified road is in range
                                            int roadArrLen = nodeSystem.nodeSystemOptions.areas[connection.AreaID].roads.Length - 1;
                                            if (IntInRangeCheck(connection.RoadID, 0, roadArrLen))
                                            {
                                                node.SetConnectionOutOfRange(connectionIndex, false, false, true);
                                                //Check if the specified node is in range
                                                int nodeArrLen = nodeSystem.nodeSystemOptions.areas[connection.AreaID].roads[connection.RoadID].nodes.Length - 1;
                                                if (IntInRangeCheck(connection.NodeID, 0, nodeArrLen))
                                                {
                                                    node.SetConnectionOutOfRange(connectionIndex, false, false, false);
                                                    //Arrow Gizmo
                                                    var nodesConnection = nodeSystem.nodeSystemOptions.areas[connection.AreaID].roads[connection.RoadID].nodes[connection.NodeID];
                                                    Gizmos.color = arrowGizmoColor;
                                                    Gizmos.DrawLine(node.Position, nodesConnection.Position);
                                                    Vector3[] arrows = GetDebugArrowBasedonDirection(node.Position, nodesConnection.Position);
                                                    Gizmos.DrawLine(arrows[0], nodesConnection.Position);
                                                    Gizmos.DrawLine(arrows[1], nodesConnection.Position);
                                                }
                                                else if (!(!logErrorOnNegativeConnections && connection.NodeID < 0))
                                                    Debug.LogError("Error in Node's connection: NodeID " + connection.NodeID.ToString() + " was out of range (0-" + areaArrLen.ToString() + ")");
                                            }
                                            else if (!(!logErrorOnNegativeConnections && connection.RoadID < 0))
                                                Debug.LogError("Error in Node's connection: RoadID " + connection.RoadID.ToString() + " was out of range (0-" + roadArrLen.ToString() + ")");
                                        }
                                        else if( !(!logErrorOnNegativeConnections && connection.AreaID < 0) )
                                            Debug.LogError( "Error in Node's connection: AreaID " + connection.AreaID.ToString() + " was out of range (0-" + areaArrLen.ToString() + ")");
 
                                    }
                                }

                                Gizmos.color = Color.grey;
                                index++;
                            }
                        index = 0;
                    }
            }
    }

    #region Debug
    [SerializeField]
    public void DebugAddNewStreet(ref NodeSystem.RoadSystem[] AreaArr)
    {
        int RoadsArrSize = AreaArr.Length;

        //Copy and expand the area array
        NodeSystem.RoadSystem[] tempArr = new NodeSystem.RoadSystem[RoadsArrSize + 1];
        AreaArr.CopyTo(tempArr, 0);
        AreaArr = tempArr;

        AreaArr[RoadsArrSize].StreetName = "Street" + RoadsArrSize;
    }

    [SerializeField]
    public void ClearAreas()
    {
        //Area level
        for (int i = 0; i < nodeSystem.nodeSystemOptions.areas.Length; ++i)
        {
            //Road level
            for (int b = 0; b < nodeSystem.nodeSystemOptions.areas[i].roads.Length; ++b)
            {
                //Delete all nodes
                Array.Clear(nodeSystem.nodeSystemOptions.areas[i].roads[b].nodes, 0, nodeSystem.nodeSystemOptions.areas[i].roads[b].nodes.Length);
            }

            //Delete all streets
            Array.Clear(nodeSystem.nodeSystemOptions.areas[i].roads, 0, nodeSystem.nodeSystemOptions.areas[i].roads.Length);
        }

        //Delete all areas
        Array.Clear(nodeSystem.nodeSystemOptions.areas, 0, nodeSystem.nodeSystemOptions.areas.Length);

        nodeSystem.nodeSystemOptions.areas = new NodeSystem.AreaSystem[1];
        nodeSystem.nodeSystemOptions.areas[0].AreaName = "New Area";
        nodeSystem.nodeSystemOptions.areas[0].roads = new NodeSystem.RoadSystem[1];
        nodeSystem.nodeSystemOptions.areas[0].roads[0].StreetName = "new Street";
        nodeSystem.nodeSystemOptions.areas[0].roads[0].nodes = new NodeSystem.Node[1];
        nodeSystem.nodeSystemOptions.areas[0].roads[0].nodes[0].bMainNode = true;

        Debug.Log("Areas cleared by the user!");
    }

    [SerializeField]
    public void FixNodeIDs()
    {
        for (int areaIndex = 0; areaIndex < nodeSystem.nodeSystemOptions.areas.Length; areaIndex++)
        {
            ref var area = ref nodeSystem.nodeSystemOptions.areas[areaIndex];

            for (int streetIndex = 0; streetIndex < area.roads.Length; streetIndex++)
            {
                ref var street = ref area.roads[streetIndex];

                for (int nodeIndex = 0; nodeIndex < street.nodes.Length; nodeIndex++)
                {
                    ref var node = ref street.nodes[nodeIndex];

                    node.ID.SetValues(areaIndex, streetIndex, nodeIndex);
                }
            }
        }

    }

    //DEBUG FUNCTIONS
    [SerializeField]
    public void Debug_PopulateAreas()
    {
        Debug.Log("Populating Areas!");

        //Copy to the area array
        NodeSystem.AreaSystem[] tempArrArea = new NodeSystem.AreaSystem[2];
        nodeSystem.nodeSystemOptions.areas.CopyTo(tempArrArea, 0);
        nodeSystem.nodeSystemOptions.areas = tempArrArea;

        //Copy to the street arrays
        NodeSystem.RoadSystem[] tempArrStreetNorwich = new NodeSystem.RoadSystem[3];
        nodeSystem.nodeSystemOptions.areas[0].roads.CopyTo(tempArrStreetNorwich, 0);
        nodeSystem.nodeSystemOptions.areas[0].roads = tempArrStreetNorwich;

        NodeSystem.RoadSystem[] tempArrStreetEdinburgh = new NodeSystem.RoadSystem[2];
        nodeSystem.nodeSystemOptions.areas[1].roads.CopyTo(tempArrStreetEdinburgh, 0);
        nodeSystem.nodeSystemOptions.areas[1].roads = tempArrStreetEdinburgh;

        //Areas
        nodeSystem.nodeSystemOptions.areas[0].AreaName = "Norwich";
        nodeSystem.nodeSystemOptions.areas[1].AreaName = "Edinburgh";

        //Streets in Norwich
        nodeSystem.nodeSystemOptions.areas[0].roads[0].StreetName = "Elm Street";
        nodeSystem.nodeSystemOptions.areas[0].roads[1].StreetName = "Hawthorne Avenue";
        nodeSystem.nodeSystemOptions.areas[0].roads[2].StreetName = "TEST Avenue";

        //Streets in Edinburgh
        nodeSystem.nodeSystemOptions.areas[1].roads[0].StreetName = "Grove Street";
        nodeSystem.nodeSystemOptions.areas[1].roads[1].StreetName = "Abbeyhill Crescent";
    }
    #endregion

}