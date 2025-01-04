﻿using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEditor;

namespace cky.TrafficSystem
{
    public class WaypointsContainer_Pedestrian : MonoBehaviour
    {
        private TrafficSystem_Pedestrian pedestrianTrafficSystem;

        public bool noPedestrian;

        public bool oneway = false;
        public bool doubleLine = false;

        [Range(0.5f, 25)]
        public float width = 2f;

        public WaypointsContainer_Pedestrian[] doNotConnectTo;

        [Range(5, 100)]
        public float limitNodeDistance = 30;  //Maximum distance between the end of one lane to the bridge of the other for automatic linking


        private float widthToUse = 2f;


        [HideInInspector]
        public List<Transform> waypoints = new List<Transform>();

        [HideInInspector]
        public WaypointsContainer_Pedestrian[] nextWay0;
        [HideInInspector]
        public WaypointsContainer_Pedestrian[] nextWay1;
        [HideInInspector]
        public int[] nextWaySide0;
        [HideInInspector]
        public int[] nextWaySide1;
        [HideInInspector]
        public int rightHand = 0;

        [HideInInspector]
        public bool bloked = false;

        [HideInInspector]
        public Transform nodeZeroCar0;
        [HideInInspector]
        public Transform nodeZeroCar1;
        [HideInInspector]
        public Transform nodeZeroWay0;
        [HideInInspector]
        public Transform nodeZeroWay1;

        [HideInInspector]
        public WaypointsContainer_Pedestrian[] directConnectSide = new WaypointsContainer_Pedestrian[2];

        Vector3 nodeBegin;
        Vector3 nodeEnd;

        [HideInInspector]
        public WpDataPedestrian wpData;


        private Vector3 oldPosition;

        float _forDistanceControl = 1;

        public void NextWaysCloseOnly()
        {
            for (int idx = 1; idx >= 0; idx--)
            {

                if (oneway && !doubleLine && idx == 0)
                    continue;

                if (directConnectSide[idx])
                    continue;

                int n = wpData.tf01.Length;

                if (n < 1)
                    continue;

                Vector3 referencia = Node(idx, waypoints.Count - 1);

                for (int i = 0; i < n; i++)
                {
                    if (!wpData.tsActive[i])
                        continue;

                    float pathDistance = Vector3.Distance(referencia, wpData.tf01[i]);
                    if (pathDistance < _forDistanceControl)
                    {
                        if (TestDoNotConnectTo(wpData.tsParent[i]) || wpData.tsParent[i].TestDoNotConnectTo(this))
                            continue;

                        if (wpData.tsOneway[i] != oneway || wpData.tsOnewayDoubleLine[i] != doubleLine || wpData.tsParent[i].transform == transform)
                            continue;

                        if (idx == 0)
                        {
                            nextWay0 = new WaypointsContainer_Pedestrian[1];
                            nextWaySide0 = new int[1];
                            nextWay0[0] = wpData.tsParent[i];
                            nextWaySide0[0] = wpData.tsSide[i];
                        }
                        else
                        {
                            nextWay1 = new WaypointsContainer_Pedestrian[1];
                            nextWaySide1 = new int[1];
                            nextWay1[0] = wpData.tsParent[i];
                            nextWaySide1[0] = wpData.tsSide[i];
                        }

                        directConnectSide[idx] = wpData.tsParent[i];

                        if (oneway)
                            wpData.tsParent[i].directConnectSide[0] = this;

                        break;
                    }
                }
            }
        }

        public void ResetWay()
        {
            nextWay0 = null;
            nextWay1 = null;

            nextWay0 = new WaypointsContainer_Pedestrian[0];
            nextWay1 = new WaypointsContainer_Pedestrian[0];

            directConnectSide[0] = null;
            directConnectSide[1] = null;

            width = Mathf.Abs(width);

        }

        public void NextWays()
        {
            for (int idx = 1; idx >= 0; idx--)
            {
                if (oneway && !doubleLine && idx == 0)
                    continue;

                if (directConnectSide[idx] != null)
                    continue;

                if (idx == 0)
                {
                    nextWay0 = new WaypointsContainer_Pedestrian[0];
                    nextWaySide0 = new int[0];
                }
                else
                {
                    nextWay1 = new WaypointsContainer_Pedestrian[0];
                    nextWaySide1 = new int[0];
                }

                Vector3 referencia = Node(idx, waypoints.Count - 1);

                ArrayList arrParent = new ArrayList();
                ArrayList arrSide = new ArrayList();

                int n = wpData.tf01.Length;

                if (n < 2)
                    continue;

                arrParent.Clear();
                arrSide.Clear();

                for (int i = 0; i < n; i++)
                {
                    if (!wpData.tsActive[i])
                        continue;

                    float pathDistance = Vector3.Distance(referencia, wpData.tf01[i]);

                    if (pathDistance < limitNodeDistance && pathDistance >= _forDistanceControl)
                    {
                        if (!wpData.tsParent[i])
                            continue;

                        if ((wpData.tsOneway[i] && !wpData.tsOnewayDoubleLine[i]) && (wpData.tsSide[i] == 0))
                            continue;

                        if ((wpData.tsOneway[i] && !wpData.tsOnewayDoubleLine[i]) && (wpData.tsParent[i].directConnectSide[0] != null))
                            continue;

                        if (TestDoNotConnectTo(wpData.tsParent[i]) || wpData.tsParent[i].TestDoNotConnectTo(this))
                            continue;

                        if (!wpData.tsOneway[i] && wpData.tsParent[i].directConnectSide[(wpData.tsSide[i] == 1) ? 0 : 1] != null)
                            continue;


                        if (wpData.tsParent[i].transform == transform)
                            continue;

                        WaypointsContainer_Pedestrian wpc = wpData.tsParent[i];

                        //Link this path with the nearby paths
                        //If the two ends of the path are close, stay with the closest one

                        if (Vector3.Distance(referencia, wpc.Node((wpData.tsSide[i] == 1) ? 0 : 1, 0)) > pathDistance || wpData.tsOneway[i])
                            if (Vector3.Distance(Node((idx == 1) ? 0 : 1, waypoints.Count - 1), wpData.tf01[i]) > pathDistance || oneway)
                            {
                                arrParent.Add(wpData.tsParent[i]);
                                arrSide.Add(wpData.tsSide[i]);

                            }
                    }
                }

                int qt = arrParent.Count;

                if (qt < 1)
                    continue;


                WaypointsContainer_Pedestrian[] _NextWays = new WaypointsContainer_Pedestrian[qt];
                int[] _NextWaysSide = new int[qt];

                for (int i = 0; i < qt; i++)
                {
                    _NextWays[i] = (WaypointsContainer_Pedestrian)arrParent[i];
                    _NextWaysSide[i] = (int)arrSide[i];
                }

                if (idx == 0)
                {
                    nextWay0 = _NextWays;
                    nextWaySide0 = _NextWaysSide;
                }
                else
                {
                    nextWay1 = _NextWays;
                    nextWaySide1 = _NextWaysSide;
                }

            }


            widthToUse = (oneway && !doubleLine) ? 0f : (rightHand == 0) ? Mathf.Abs(width) : -Mathf.Abs(width); ;

            //Block path that has no exit
            bloked = ((!oneway && (nextWay0.Length < 1 || nextWay1.Length < 1)) || (oneway && (nextWay0.Length < 1 && nextWay1.Length < 1)));   // If one of my ends is not linked to another route, ban me


        }

        public bool TestDoNotConnectTo(WaypointsContainer_Pedestrian t)
        {
            if (doNotConnectTo == null) return false;
            if (doNotConnectTo.Length == 0) return false;

            for (int d = 0; d < doNotConnectTo.Length; d++)
            {
                if (doNotConnectTo[d] == t)
                    return true;
            }

            return false;

        }

        private float GetAngulo180(Transform origem, Vector3 target)
        {

            return Vector3.Angle(target - origem.position, origem.forward);

        }


        public void InvertNodesDirection()
        {

            widthToUse = Mathf.Abs(width);

        }

        float _timer;

        public void RefreshAllWayPoints()
        {
            if (Time.time - _timer < 0.2f) return;
            _timer = Time.time;

            if (!pedestrianTrafficSystem)
            {
                pedestrianTrafficSystem = FindObjectOfType<TrafficSystem_Pedestrian>();

#if UNITY_EDITOR
                if (!pedestrianTrafficSystem)
                    pedestrianTrafficSystem = (TrafficSystem_Pedestrian)AssetDatabase.LoadAssetAtPath("Assets/cky - Traffic System/Resources/Traffic System/Traffic System - Pedestrian.prefab", (typeof(TrafficSystem_Car)));
#endif

                if (!pedestrianTrafficSystem)
                    Debug.LogError("Traffic System - Pedestrian.prefab was not found in 'Assets/cky - Traffic System/Resources/Traffic System'");
            }

            pedestrianTrafficSystem.UpdateAllWayPoints();

        }


#if UNITY_EDITOR

        void OnDrawGizmos()
        {
            if (!oneway) doubleLine = false;

            bool isPlay = Application.isPlaying;
            int wCount = waypoints.Count;


            if (transform.childCount != waypoints.Count)
            {
                RefreshAllWayPoints();
                return;
            }

            WaypointsSetAngle();

            if (transform.childCount < 1) return;

            if (!isPlay && wCount > 1)
            {

                if ((waypoints[wCount - 1].localPosition != nodeEnd) || (waypoints[0].localPosition != nodeBegin))
                {
                    if (UnityEditor.Selection.activeGameObject)
                        if (UnityEditor.Selection.activeGameObject.transform.parent == this.transform)
                            RefreshAllWayPoints();
                }


                nodeBegin = waypoints[0].localPosition;
                nodeEnd = waypoints[wCount - 1].localPosition;

            }

            widthToUse = (oneway && !doubleLine) ? 0.1f : (rightHand == 0) ? Mathf.Abs(width) : -Mathf.Abs(width); ;

            for (int i = 0; i < wCount; i++)
            {

                if (noPedestrian)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(waypoints[i].position, 0.4f);
                }
                else
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawSphere(waypoints[i].position, 0.6f);
                }

                if (wCount < 2) return;


                if (i < wCount - 1)
                {

                    if (oneway && !doubleLine)
                        Gizmos.color = Color.yellow;

                    Gizmos.color = new Color(1f, 1f, 0f, 0.5f); // Central Line
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);

                    Vector3 offset = waypoints[i].transform.right * widthToUse;
                    Vector3 offsetTo = waypoints[i + 1].transform.right * widthToUse;

                    // White line
                    if (i == 0)
                    {
                        //Gizmos.color = Gizmos.color = new Color(1.0f, 0.35f, 1.0f, 1.0f);
                        Gizmos.color = Color.cyan; // Connection
                        for (int t = 0; t < nextWay0.Length; t++)
                            if (!oneway)
                                Gizmos.DrawLine(Node(0, wCount - 1), nextWay0[t].Node(nextWaySide0[t], 0));



                        if (!isPlay)
                        {

                            //Gizmos.color = new Color(0.0f, 0.35f, 1.0f, 1.0f);
                            Gizmos.color = Color.cyan; // Back Arrows

                            if (oneway)
                            {
                                int m = -1;
                                if (doubleLine)
                                {
                                    Gizmos.DrawLine(waypoints[i].position + offset, waypoints[i].position + (waypoints[i].right * widthToUse * 0.8f) + waypoints[i].forward * m);
                                    Gizmos.DrawLine(waypoints[i].position + offset, waypoints[i].position + (waypoints[i].right * widthToUse * 1.2f) + waypoints[i].forward * m);
                                    Gizmos.DrawLine(waypoints[i].position - offset, waypoints[i].position - (waypoints[i].right * widthToUse * 0.8f) + waypoints[i].forward * m);
                                    Gizmos.DrawLine(waypoints[i].position - offset, waypoints[i].position - (waypoints[i].right * widthToUse * 1.2f) + waypoints[i].forward * m);
                                }
                                else
                                {
                                    Gizmos.DrawLine(waypoints[i].position, waypoints[i].position + (waypoints[i].right * 0.4f) + waypoints[i].forward * m);
                                    Gizmos.DrawLine(waypoints[i].position, waypoints[i].position - (waypoints[i].right * 0.4f) + waypoints[i].forward * m);
                                }
                            }
                            else
                            {
                                Gizmos.DrawLine(waypoints[i].position + offset, waypoints[i].position + (waypoints[i].right * widthToUse * 0.8f) + waypoints[i].forward * -1);
                                Gizmos.DrawLine(waypoints[i].position + offset, waypoints[i].position + (waypoints[i].right * widthToUse * 1.2f) + waypoints[i].forward * -1);
                                Gizmos.DrawLine(waypoints[i].position - offset, waypoints[i].position - (waypoints[i].right * widthToUse * 0.8f) + waypoints[i].forward * 1);
                                Gizmos.DrawLine(waypoints[i].position - offset, waypoints[i].position - (waypoints[i].right * widthToUse * 1.2f) + waypoints[i].forward * 1);
                            }
                        }


                    }





                    if (!oneway || doubleLine)
                    {

                        Gizmos.color = noPedestrian ? Color.blue : Color.yellow; // Left - Right Lines

                        Gizmos.DrawLine(waypoints[i].position + offset, waypoints[i].position - offset);

                        Gizmos.color = (oneway && doubleLine) ? Color.magenta : Color.yellow;

                        if (noPedestrian) Gizmos.color = Color.blue;
                        Gizmos.DrawLine(waypoints[i].position + offset, waypoints[i + 1].position + offsetTo);
                        Gizmos.DrawLine(waypoints[i].position - offset, waypoints[i + 1].position - offsetTo);

                    }

                }
                else
                {

                    // Last node

                    Vector3 offset = waypoints[i].transform.right * widthToUse;

                    Gizmos.color = Color.yellow; // Last Line

                    Gizmos.DrawLine(waypoints[i].position + offset, waypoints[i].position - offset);


                    if (!isPlay)
                    {

                        //Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
                        Gizmos.color = Color.yellow; // Front Arrows

                        if (oneway)
                        {
                            int m = (rightHand == 0) ? -1 : 1;
                            if (doubleLine)
                            {
                                Gizmos.DrawLine(waypoints[i].position + offset, waypoints[i].position + (waypoints[i].right * widthToUse * 0.8f) + waypoints[i].forward * m);
                                Gizmos.DrawLine(waypoints[i].position + offset, waypoints[i].position + (waypoints[i].right * widthToUse * 1.2f) + waypoints[i].forward * m);
                                Gizmos.DrawLine(waypoints[i].position - offset, waypoints[i].position - (waypoints[i].right * widthToUse * 0.8f) + waypoints[i].forward * m);
                                Gizmos.DrawLine(waypoints[i].position - offset, waypoints[i].position - (waypoints[i].right * widthToUse * 1.2f) + waypoints[i].forward * m);
                            }
                            else
                            {
                                Gizmos.DrawLine(waypoints[i].position, waypoints[i].position + (waypoints[i].right * 0.4f) + waypoints[i].forward * m);
                                Gizmos.DrawLine(waypoints[i].position, waypoints[i].position - (waypoints[i].right * 0.4f) + waypoints[i].forward * m);
                            }
                        }
                        else
                        {
                            Gizmos.DrawLine(waypoints[i].position + offset, waypoints[i].position + (waypoints[i].right * widthToUse * 0.8f) + waypoints[i].forward * -1);
                            Gizmos.DrawLine(waypoints[i].position + offset, waypoints[i].position + (waypoints[i].right * widthToUse * 1.2f) + waypoints[i].forward * -1);
                            Gizmos.DrawLine(waypoints[i].position - offset, waypoints[i].position - (waypoints[i].right * widthToUse * 0.8f) + waypoints[i].forward * 1);
                            Gizmos.DrawLine(waypoints[i].position - offset, waypoints[i].position - (waypoints[i].right * widthToUse * 1.2f) + waypoints[i].forward * 1);
                        }

                    }

                    Gizmos.color = Color.cyan; // Connection

                    for (int t = 0; t < nextWay1.Length; t++)
                    {

                        Gizmos.DrawLine(Node(1, i), nextWay1[t].Node(nextWaySide1[t], 0));

                        if (doubleLine)
                            Gizmos.DrawLine(Node(0, i), nextWay1[t].Node(nextWaySide1[t], 0));

                    }

                }

            }


            /*
            Gizmos.color = Color.cyan;

            if (rightHand == 0 && nodeZeroCar1)
                Gizmos.DrawLine(Node(1,0) , nodeZeroCar1.position);
            else if (rightHand != 0 && nodeZeroCar0)
                Gizmos.DrawLine(Node(0, 0), nodeZeroCar0.position);


            if (rightHand == 0 && nodeZeroCar0)
                Gizmos.DrawLine(Node(0,0), nodeZeroCar0.position);
            else if (rightHand != 0 && nodeZeroCar1)
                Gizmos.DrawLine(Node(1,0), nodeZeroCar1.position);
            */

        }

#endif

        public int GetTotalNodes()
        {
            return waypoints.Count - 1;
        }


        public Transform GetNodeZeroCar(int side)
        {
            return (side == 0) ? nodeZeroCar0 : nodeZeroCar1;
        }
        public Transform GetNodeZeroWay(int side)
        {
            return (side == 0) ? nodeZeroWay0 : nodeZeroWay1;
        }

        //public Transform GetNodeZeroOldWay(int side)
        //{

        //    return (side == 0) ? nodeZeroCar0.GetComponent<TrafficPedestrian>().myOldWay : nodeZeroCar1.GetComponent<TrafficPedestrian>().myOldWay;

        //}

        public bool SetNodeZero(int side, Transform nodeWay, Transform nodeCar, bool force = false)
        {

            if (side == 0)
            {
                if (nodeZeroCar0 == null || force)
                {
                    nodeZeroWay0 = nodeWay;
                    nodeZeroCar0 = nodeCar;
                }
                return nodeZeroCar0 == nodeCar;
            }
            else
            {
                if (nodeZeroCar1 == null || force)
                {
                    nodeZeroWay1 = nodeWay;
                    nodeZeroCar1 = nodeCar;
                }
                return nodeZeroCar1 == nodeCar;
            }

        }

        public bool UnSetNodeZero(int side, Transform carTransform, bool force = false)
        {

            if (side == 0)
            {
                if (nodeZeroCar0 == carTransform || force)
                {
                    nodeZeroWay0 = null;
                    nodeZeroCar0 = null;
                }
                return nodeZeroCar0 == null;
            }
            else
            {
                if (nodeZeroCar1 == carTransform || force)
                {
                    nodeZeroWay1 = null;
                    nodeZeroCar1 = null;
                }
                return nodeZeroCar1 == null;
            }

        }



        //public bool BookNodeZero(TrafficPedestrian pedestrian)
        //{

        //    if (pedestrian.sideAtual == 0)
        //    {

        //        if (SetNodeZero(0, pedestrian.myOldWay, pedestrian.transform))
        //            return true;
        //        else
        //        {
        //            if (pedestrian.nodeSteerCarefully2 == false && pedestrian.nodeSteerCarefully == false && nodeZeroCar0.GetComponent<TrafficPedestrian>().Get_avanceNode() && pedestrian.GetBehind() != nodeZeroCar0)
        //            {
        //                SetNodeZero(0, pedestrian.myOldWay, pedestrian.transform, true);
        //            }

        //            //The starting node of the path is already reserved for another car. So the car that called this procedure must wait.
        //            return ((nodeZeroCar0 == pedestrian.transform) || (nodeZeroWay0 == pedestrian.myOldWay && pedestrian.myOldSideAtual == nodeZeroCar0.GetComponent<TrafficPedestrian>().myOldSideAtual));
        //        }

        //    }
        //    else
        //    {

        //        if (SetNodeZero(1, pedestrian.myOldWay, pedestrian.transform))
        //            return true;
        //        else
        //        {
        //            if (pedestrian.nodeSteerCarefully == false && nodeZeroCar1.GetComponent<TrafficPedestrian>().Get_avanceNode() && pedestrian.GetBehind() != nodeZeroCar1)
        //            {
        //                SetNodeZero(1, pedestrian.myOldWay, pedestrian.transform, true);
        //            }

        //            //The starting node of the path is already reserved for another car. So the car that called this procedure must wait.                
        //            return ((nodeZeroCar1 == pedestrian.transform) || (nodeZeroWay1 == pedestrian.myOldWay && pedestrian.myOldSideAtual == nodeZeroCar1.GetComponent<TrafficPedestrian>().myOldSideAtual));
        //        }

        //    }

        //}




        public Vector3 AvanceNode(int side, int idx, float mts = 1)
        {
            /*
            Returns a Vector3 that is a position in front of the specified node.
            The value is specified in the mts parameter. This value can be positive or negative.
            */

            if ((!oneway && side == 0) || (oneway && rightHand != 0))
            {
                int i = (waypoints.Count - 1) - idx;
                return waypoints[i].position - (waypoints[i].transform.forward * mts) - (waypoints[i].transform.right * ((doubleLine && side == 0) ? -widthToUse : widthToUse));
            }
            else
            {
                return waypoints[idx].position + (waypoints[idx].transform.forward * mts) + (waypoints[idx].transform.right * ((doubleLine && side == 0) ? -widthToUse : widthToUse));
            }


        }

        public Quaternion NodeRotation(int side, int idx)
        {

            if ((!oneway && side == 1) || (oneway && rightHand == 0))     // 0 = do Inicio para o fim  e   1 = do fim para o inicio
                return Quaternion.LookRotation(waypoints[idx + 1].position - waypoints[idx].position);
            else
            {
                int i = (waypoints.Count - 1) - idx;
                return Quaternion.LookRotation(waypoints[i - 1].position - waypoints[i].position);
            }

        }

        public Vector3 Node(int side, int idx, float nodeSteerCarefully = 0)
        {

            /*
             Returns a Vector3 referring to the position of the specified node
             Note that the real nodes are in the middle. The nodes that will actually be followed are relative positions, shown in red line (Gizmos)
            */

            if (oneway)
            {

                int i = (rightHand == 0) ? idx : (waypoints.Count - 1) - idx;

                if (!doubleLine)
                    return waypoints[i].position;
                else
                    return waypoints[i].position + (((side == 1) ? (waypoints[i].transform.right * widthToUse) : (waypoints[i].transform.right * -widthToUse)));

            }
            else
            {
                int i = (side == 1) ? idx : (waypoints.Count - 1) - idx;

                if (nodeSteerCarefully > 0 && idx == 0)
                {
                    if (side == 1)
                        return (waypoints[i].position - (waypoints[i].transform.forward * nodeSteerCarefully)) + (waypoints[i].transform.right * ((side == 1) ? widthToUse : -widthToUse));
                    else
                        return (waypoints[i].position + (waypoints[i].transform.forward * nodeSteerCarefully)) + (waypoints[i].transform.right * ((side == 1) ? widthToUse : -widthToUse));
                }
                else
                    return waypoints[i].position + waypoints[i].transform.right * ((side == 1) ? widthToUse : -widthToUse);
            }



        }



        public void GetWaypoints()
        {

            waypoints = new List<Transform>();

            Transform[] allTransforms = transform.GetComponentsInChildren<Transform>();

            for (int i = 1; i < allTransforms.Length; i++)
            {
                //TIRAR
                //allTransforms[i].name = this.name + " - " + i.ToString("00");
                waypoints.Add(allTransforms[i]);

            }

            WaypointsSetAngle();


        }


        public void WaypointsSetAngle()
        {


            int wCount = waypoints.Count;

            if (wCount > 1)
            {
                waypoints[0].LookAt(waypoints[1]);

                for (int i = 1; i < wCount - 1; i++)
                    waypoints[i].rotation = Quaternion.LookRotation(waypoints[i + 1].position - waypoints[i - 1].position);

                waypoints[wCount - 1].rotation = Quaternion.LookRotation(waypoints[wCount - 1].position - waypoints[wCount - 2].position);

            }

        }





    }

}