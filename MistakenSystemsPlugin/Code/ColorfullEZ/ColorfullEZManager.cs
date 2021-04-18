using Exiled.API.Enums;
using Gamer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Gamer.Mistaken.ColorfullEZ
{
    public static class ColorfullEZManager
    {
        //Pos, Size, Rot
        public static Dictionary<RoomType, List<(Vector3, Vector3, Vector3)>> keycardRooms = new Dictionary<RoomType, List<(Vector3, Vector3, Vector3)>>()
        {
            {
                RoomType.EzCrossing, new List<(Vector3, Vector3, Vector3)>()
                {
                    (new Vector3(2.946f, 3.02f, 2.946f), new Vector3(8.1f, 20f, 0.05f), Vector3.up * 225f),
                    (new Vector3(2.946f, 3.02f, -2.946f), new Vector3(8.1f, 20f, 0.05f), Vector3.up * -225f),
                    (new Vector3(-2.946f, 3.02f, 2.946f), new Vector3(8.1f, 20f, 0.05f), Vector3.up * -45f),
                    (new Vector3(-2.946f, 3.02f, -2.946f), new Vector3(8.1f, 20f, 0.05f), Vector3.up * 45f),

                    (new Vector3(2.946f, 2.696f, 2.946f), new Vector3(8.1f, 20f, 0.05f), Vector3.up * 225f),
                    (new Vector3(2.946f, 2.696f, -2.946f), new Vector3(8.1f, 20f, 0.05f), Vector3.up * -225f),
                    (new Vector3(-2.946f, 2.696f, 2.946f), new Vector3(8.1f, 20f, 0.05f), Vector3.up * -45f),
                    (new Vector3(-2.946f, 2.696f, -2.946f), new Vector3(8.1f, 20f, 0.05f), Vector3.up * 45f),

                    (new Vector3(2.946f, 0.55f, 2.946f), new Vector3(8.1f, 80f, 0.05f), Vector3.up * 225f),
                    (new Vector3(2.946f, 0.55f, -2.946f), new Vector3(8.1f, 80f, 0.05f), Vector3.up * -225f),
                    (new Vector3(-2.946f, 0.55f, 2.946f), new Vector3(8.1f, 80f, 0.05f), Vector3.up * -45f),
                    (new Vector3(-2.946f, 0.55f, -2.946f), new Vector3(8.1f, 80f, 0.05f), Vector3.up * 45f),

                    (new Vector3(2.293f, 0.55f, 3.65f), new Vector3(0.5f, 80f, 0.05f), Vector3.up * 180f),
                    (new Vector3(2.293f, 0.55f, -3.65f), new Vector3(0.5f, 80f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-2.293f, 0.55f, 3.65f), new Vector3(0.5f, 80f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-2.293f, 0.55f, -3.65f), new Vector3(0.5f, 80f, 0.05f), Vector3.up * 0f),
                    (new Vector3(3.65f, 0.55f, 2.293f), new Vector3(0.5f, 80f, 0.05f), Vector3.up * -90f),
                    (new Vector3(3.65f, 0.55f, -2.293f), new Vector3(0.5f, 80f, 0.05f), Vector3.up * 90f),
                    (new Vector3(-3.65f, 0.55f, 2.293f), new Vector3(0.5f, 80f, 0.05f), Vector3.up * -90f),
                    (new Vector3(-3.65f, 0.55f, -2.293f), new Vector3(0.5f, 80f, 0.05f), Vector3.up * 90f),

                    (new Vector3(2.293f, 2.696f, 3.65f), new Vector3(0.5f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(2.293f, 2.696f, -3.65f), new Vector3(0.5f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-2.293f, 2.696f, 3.65f), new Vector3(0.5f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-2.293f, 2.696f, -3.65f), new Vector3(0.5f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(3.65f, 2.696f, 2.293f), new Vector3(0.5f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(3.65f, 2.696f, -2.293f), new Vector3(0.5f, 20f, 0.05f), Vector3.up * 90f),
                    (new Vector3(-3.65f, 2.696f, 2.293f), new Vector3(0.5f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(-3.65f, 2.696f, -2.293f), new Vector3(0.5f, 20f, 0.05f), Vector3.up * 90f),

                    (new Vector3(2.293f, 3.02f, 3.65f), new Vector3(0.5f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(2.293f, 3.02f, -3.65f), new Vector3(0.5f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-2.293f, 3.02f, 3.65f), new Vector3(0.5f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-2.293f, 3.02f, -3.65f), new Vector3(0.5f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(3.65f, 3.02f, 2.293f), new Vector3(0.5f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(3.65f, 3.02f, -2.293f), new Vector3(0.5f, 20f, 0.05f), Vector3.up * 90f),
                    (new Vector3(-3.65f, 3.02f, 2.293f), new Vector3(0.5f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(-3.65f, 3.02f, -2.293f), new Vector3(0.5f, 20f, 0.05f), Vector3.up * 90f),

                    (new Vector3(6.8831f, 2.696f, 2.293f), new Vector3(28.6f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(6.8831f, 2.696f, -2.293f), new Vector3(28.6f, 20f, 0.05f), Vector3.up * 90f),
                    (new Vector3(-6.8831f, 2.696f, 2.293f), new Vector3(28.6f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(-6.8831f, 2.696f, -2.293f), new Vector3(28.6f, 20f, 0.05f), Vector3.up * 90f),
                    (new Vector3(2.293f, 2.696f, 6.8831f), new Vector3(28.6f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(2.293f, 2.696f, -6.8831f), new Vector3(28.6f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-2.293f, 2.696f, 6.8831f), new Vector3(28.6f, 20f, 0.05f), Vector3.up * 360f),
                    (new Vector3(-2.293f, 2.696f, -6.8831f), new Vector3(28.6f, 20f, 0.05f), Vector3.up * 360f),
                    (new Vector3(10.145f, 2.696f, 1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.145f, 2.696f, -1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-10.145f, 2.696f, 1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 360f),
                    (new Vector3(-10.145f, 2.696f, -1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 360f),
                    (new Vector3(1.64f, 2.696f, 10.145f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(1.64f, 2.696f, -10.145f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 90f),
                    (new Vector3(-1.64f, 2.696f, 10.145f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(-1.64f, 2.696f, -10.145f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 90f),

                    (new Vector3(6.8831f, 3.02f, 2.293f), new Vector3(28.6f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(6.8831f, 3.02f, -2.293f), new Vector3(28.6f, 20f, 0.05f), Vector3.up * 90f),
                    (new Vector3(-6.8831f, 3.02f, 2.293f), new Vector3(28.6f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(-6.8831f, 3.02f, -2.293f), new Vector3(28.6f, 20f, 0.05f), Vector3.up * 90f),
                    (new Vector3(2.293f, 3.02f, 6.8831f), new Vector3(28.6f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(2.293f, 3.02f, -6.8831f), new Vector3(28.6f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-2.293f, 3.02f, 6.8831f), new Vector3(28.6f, 20f, 0.05f), Vector3.up * 360f),
                    (new Vector3(-2.293f, 3.02f, -6.8831f), new Vector3(28.6f, 20f, 0.05f), Vector3.up * 360f),
                    (new Vector3(10.145f, 3.02f, 1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.145f, 3.02f, -1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-10.145f, 3.02f, 1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 360f),
                    (new Vector3(-10.145f, 3.02f, -1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 360f),
                    (new Vector3(1.64f, 3.02f, 10.145f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(1.64f, 3.02f, -10.145f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 90f),
                    (new Vector3(-1.64f, 3.02f, 10.145f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(-1.64f, 3.02f, -10.145f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 90f),

                    (new Vector3(6.8831f, 0.55f, 2.293f), new Vector3(28.6f, 80f, 0.05f), Vector3.up * -90f),
                    (new Vector3(6.8831f, 0.55f, -2.293f), new Vector3(28.6f, 80f, 0.05f), Vector3.up * 90f),
                    (new Vector3(-6.8831f, 0.55f, 2.293f), new Vector3(28.6f, 80f, 0.05f), Vector3.up * -90f),
                    (new Vector3(-6.8831f, 0.55f, -2.293f), new Vector3(28.6f, 80f, 0.05f), Vector3.up * 90f),
                    (new Vector3(2.293f, 0.55f, 6.8831f), new Vector3(28.6f, 80f, 0.05f), Vector3.up * 180f),
                    (new Vector3(2.293f, 0.55f, -6.8831f), new Vector3(28.6f, 80f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-2.293f, 0.55f, 6.8831f), new Vector3(28.6f, 80f, 0.05f), Vector3.up * 360f),
                    (new Vector3(-2.293f, 0.55f, -6.8831f), new Vector3(28.6f, 80f, 0.05f), Vector3.up * 360f),
                    (new Vector3(10.145f, 0.55f, 1.64f), new Vector3(5.8f, 80f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.145f, 0.55f, -1.64f), new Vector3(5.8f, 80f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-10.145f, 0.55f, 1.64f), new Vector3(5.8f, 80f, 0.05f), Vector3.up * 360f),
                    (new Vector3(-10.145f, 0.55f, -1.64f), new Vector3(5.8f, 80f, 0.05f), Vector3.up * 360f),
                    (new Vector3(1.64f, 0.55f, 10.145f), new Vector3(5.8f, 80f, 0.05f), Vector3.up * -90f),
                    (new Vector3(1.64f, 0.55f, -10.145f), new Vector3(5.8f, 80f, 0.05f), Vector3.up * 90f),
                    (new Vector3(-1.64f, 0.55f, 10.145f), new Vector3(5.8f, 80f, 0.05f), Vector3.up * -90f),
                    (new Vector3(-1.64f, 0.55f, -10.145f), new Vector3(5.8f, 80f, 0.05f), Vector3.up * 90f),
                }
            },
            {
                RoomType.EzStraight, new List<(Vector3, Vector3, Vector3)>()
                {
                    (new Vector3(10.16f, 0.55f, 1.64f), new Vector3(5.8f, 80f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.16f, 0.55f, -1.64f), new Vector3(5.8f, 80f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-10.13f, 0.55f, 1.64f), new Vector3(5.8f, 80f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.13f, 0.55f, -1.64f), new Vector3(5.8f, 80f, 0.05f), Vector3.up * 0f),

                    (new Vector3(10.16f, 2.696f, 1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.16f, 2.696f, -1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-10.13f, 2.696f, 1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.13f, 2.696f, -1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 0f),

                    (new Vector3(10.16f, 3.02f, 1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.16f, 3.02f, -1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-10.13f, 3.02f, 1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.13f, 3.02f, -1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 0f),

                    (new Vector3(0f, 0.55f, 2.291f), new Vector3(90f, 80f, 0.05f), Vector3.up * -90f),
                    (new Vector3(0f, 0.55f, -2.291f), new Vector3(90f, 80f, 0.05f), Vector3.up * 90f),
                    (new Vector3(0f, 2.696f, 2.291f), new Vector3(90f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(0f, 2.696f, -2.291f), new Vector3(90f, 20f, 0.05f), Vector3.up * 90f),
                    (new Vector3(0f, 3.02f, 2.291f), new Vector3(90f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(0f, 3.02f, -2.291f), new Vector3(90f, 20f, 0.05f), Vector3.up * 90f)
                }
            },
            {
                RoomType.EzCollapsedTunnel, new List<(Vector3, Vector3, Vector3)>()
                {
                    (new Vector3(10.15f, 0.55f, 1.64f), new Vector3(5.8f, 80f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.15f, 0.55f, -1.64f), new Vector3(5.8f, 80f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-10.1396f, 0.55f, 1.64f), new Vector3(5.8f, 80f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.1396f, 0.55f, -1.64f), new Vector3(5.8f, 80f, 0.05f), Vector3.up * 0f),

                    (new Vector3(10.15f, 2.696f, 1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.15f, 2.696f, -1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-10.1396f, 2.696f, 1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.1396f, 2.696f, -1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 0f),

                    (new Vector3(10.15f, 3.02f, 1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.15f, 3.02f, -1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-10.1396f, 3.02f, 1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.1396f, 3.02f, -1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 0f),

                    (new Vector3(0f, 0.55f, 2.291f), new Vector3(90f, 80f, 0.05f), Vector3.up * -90f),
                    (new Vector3(0f, 0.55f, -2.291f), new Vector3(90f, 80f, 0.05f), Vector3.up * 90f),

                    (new Vector3(0f, 2.696f, 2.291f), new Vector3(90f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(0f, 2.696f, -2.291f), new Vector3(90f, 20f, 0.05f), Vector3.up * 90f),

                    (new Vector3(0f, 3.02f, 2.291f), new Vector3(90f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(0f, 3.02f, -2.291f), new Vector3(90f, 20f, 0.05f), Vector3.up * 90f)
                }
            },
            {
                RoomType.EzCafeteria, new List<(Vector3, Vector3, Vector3)>()
                {
                    (new Vector3(10.143f, 0.55f, 1.64f), new Vector3(5.8f, 80f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.143f, 0.55f, -1.64f), new Vector3(5.8f, 80f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-10.143f, 0.55f, 1.64f), new Vector3(5.8f, 80f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.143f, 0.55f, -1.64f), new Vector3(5.8f, 80f, 0.05f), Vector3.up * 0f),

                    (new Vector3(10.143f, 2.696f, 1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.143f, 2.696f, -1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-10.143f, 2.696f, 1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.143f, 2.696f, -1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 0f),

                    (new Vector3(10.143f, 3.02f, 1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.143f, 3.02f, -1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-10.143f, 3.02f, 1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.143f, 3.02f, -1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 0f),

                    (new Vector3(0f, 0.55f, 2.291f), new Vector3(90f, 80f, 0.05f), Vector3.up * -90f),
                    (new Vector3(0f, 0.55f, -2.291f), new Vector3(90f, 80f, 0.05f), Vector3.up * 90f),

                    (new Vector3(0f, 2.696f, 2.291f), new Vector3(90f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(0f, 2.696f, -2.291f), new Vector3(90f, 20f, 0.05f), Vector3.up * 90f),

                    (new Vector3(0f, 3.02f, 2.291f), new Vector3(90f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(0f, 3.02f, -2.291f), new Vector3(90f, 20f, 0.05f), Vector3.up * 90f)
                }
            },
            {
                RoomType.EzPcs, new List<(Vector3, Vector3, Vector3)>()
                {
                    (new Vector3(10.133f, 0.55f, 5.68f), new Vector3(41f, 80f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-10.152f, 0.55f, 5.68f), new Vector3(41f, 80f, 0.05f), Vector3.up * 0f),
                    (new Vector3(10.133f, 2.696f, 5.68f), new Vector3(41f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-10.152f, 2.696f, 5.68f), new Vector3(41f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(10.133f, 3.02f, 5.68f), new Vector3(41f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-10.152f, 3.02f, 5.68f), new Vector3(41f, 20f, 0.05f), Vector3.up * 0f),

                    (new Vector3(0f, 0.55f, 10.143f), new Vector3(90f, 80f, 0.05f), Vector3.up * -90f),
                    (new Vector3(0f, 0.55f, -5.89f), new Vector3(90f, 80f, 0.05f), Vector3.up * 90f),
                    (new Vector3(0f, 2.696f, 10.143f), new Vector3(90f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(0f, 2.696f, -5.89f), new Vector3(90f, 20f, 0.05f), Vector3.up * 90f),
                    (new Vector3(0f, 3.02f, 10.143f), new Vector3(90f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(0f, 3.02f, -5.89f), new Vector3(90f, 20f, 0.05f), Vector3.up * 90f),

                    (new Vector3(10.133f, 0.55f, -3.62f), new Vector3(23f, 80f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-10.1543f, 0.55f, -3.62f), new Vector3(23f, 80f, 0.05f), Vector3.up * 0f),
                    (new Vector3(10.133f, 2.696f, -3.62f), new Vector3(23f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-10.1543f, 2.696f, -3.62f), new Vector3(23f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(10.133f, 3.02f, -3.62f), new Vector3(23f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-10.1543f, 3.02f, -3.62f), new Vector3(23f, 20f, 0.05f), Vector3.up * 0f),

                    (new Vector3(-10.152f, 0.55f, 1.09f), new Vector3(1f, 80f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.152f, 0.55f, -1.09f), new Vector3(1f, 80f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.152f, 2.696f, 1.09f), new Vector3(1f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.152f, 2.696f, -1.09f), new Vector3(1f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.152f, 3.02f, 1.09f), new Vector3(1f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.152f, 3.02f, -1.09f), new Vector3(1f, 20f, 0.05f), Vector3.up * 0f),

                    (new Vector3(10.133f, 0.55f, 1.09f), new Vector3(1f, 80f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.133f, 0.55f, -1.09f), new Vector3(1f, 80f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.133f, 2.696f, 1.09f), new Vector3(1f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.133f, 2.696f, -1.09f), new Vector3(1f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.133f, 3.02f, 1.09f), new Vector3(1f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.133f, 3.02f, -1.09f), new Vector3(1f, 20f, 0.05f), Vector3.up * 180f)
                }
            },
            {
                RoomType.EzCurve, new List<(Vector3, Vector3, Vector3)>()
                {
                    (new Vector3(10.143f, 0.55f, 1.64f), new Vector3(5.8f, 80f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.143f, 0.55f, -1.64f), new Vector3(5.8f, 80f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.143f, 2.696f, 1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.143f, 2.696f, -1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.143f, 3.02f, 1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.143f, 3.02f, -1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 180f),

                    (new Vector3(1.64f, 0.55f, 10.143f), new Vector3(5.8f, 80f, 0.05f), Vector3.up * -90f),
                    (new Vector3(-1.64f, 0.55f, 10.143f), new Vector3(5.8f, 80f, 0.05f), Vector3.up * -90f),
                    (new Vector3(1.64f, 2.696f, 10.143f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(-1.64f, 2.696f, 10.143f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(1.64f, 3.02f, 10.143f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(-1.64f, 3.02f, 10.143f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * -90f),

                    (new Vector3(-2.293f, 0.55f, 7.04f), new Vector3(30f, 80f, 0.05f), Vector3.up * 0f),
                    (new Vector3(7.04f, 0.55f, -2.293f), new Vector3(30f, 80f, 0.05f), Vector3.up * 90f),
                    (new Vector3(-2.293f, 2.696f, 7.04f), new Vector3(30f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(7.04f, 2.696f, -2.293f), new Vector3(30f, 20f, 0.05f), Vector3.up * 90f),
                    (new Vector3(-2.293f, 3.02f, 7.04f), new Vector3(30f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(7.04f, 3.02f, -2.293f), new Vector3(30f, 20f, 0.05f), Vector3.up * 90f),

                    (new Vector3(2.289f, 0.55f, 6.31f), new Vector3(35f, 80f, 0.05f), Vector3.up * 180f),
                    (new Vector3(6.31f, 0.55f, 2.29f), new Vector3(35f, 80f, 0.05f), Vector3.up * -90f),
                    (new Vector3(2.289f, 2.696f, 6.31f), new Vector3(35f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(6.31f, 2.696f, 2.29f), new Vector3(35f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(2.289f, 3.02f, 6.31f), new Vector3(35f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(6.31f, 3.02f, 2.29f), new Vector3(35f, 20f, 0.05f), Vector3.up * -90f),

                    (new Vector3(3.599f, 0.55f, -4.01f), new Vector3(15f, 80f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-4.01f, 0.55f, 3.599f), new Vector3(15f, 80f, 0.05f), Vector3.up * -90f),
                    (new Vector3(3.599f, 2.696f, -4.01f), new Vector3(15f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-4.01f, 2.696f, 3.599f), new Vector3(15f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(3.599f, 3.02f, -4.01f), new Vector3(15f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-4.01f, 3.02f, 3.599f), new Vector3(15f, 20f, 0.05f), Vector3.up * -90f),

                    (new Vector3(1f, 0.55f, -5.563f), new Vector3(25f, 80f, 0.05f), Vector3.up * 90f),
                    (new Vector3(-5.563f, 0.55f, 1f), new Vector3(25f, 80f, 0.05f), Vector3.up * -0f),
                    (new Vector3(1f, 2.696f, -5.563f), new Vector3(25f, 20f, 0.05f), Vector3.up * 90f),
                    (new Vector3(-5.563f, 2.696f, 1f), new Vector3(25f, 20f, 0.05f), Vector3.up * -0f),
                    (new Vector3(1f, 3.02f, -5.563f), new Vector3(25f, 20f, 0.05f), Vector3.up * 90f),
                    (new Vector3(-5.563f, 3.02f, 1f), new Vector3(25f, 20f, 0.05f), Vector3.up * 0f),

                    (new Vector3(-3.6f, 0.55f, -3.6f), new Vector3(25f, 80f, 0.05f), Vector3.up * 225f),
                    (new Vector3(-3.6f, 2.696f, -3.6f), new Vector3(25f, 20f, 0.05f), Vector3.up * 225f),
                    (new Vector3(-3.6f, 3.02f, -3.6f), new Vector3(25f, 20f, 0.05f), Vector3.up * 225f),

                    (new Vector3(2.290f, 0.55f, 2.4f), new Vector3(1f, 80f, 0.05f), Vector3.up * 180f),
                    (new Vector3(2.4f, 0.55f, 2.291f), new Vector3(1f, 80f, 0.05f), Vector3.up * -90f),
                    (new Vector3(2.290f, 2.696f, 2.4f), new Vector3(1f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(2.4f, 2.696f, 2.291f), new Vector3(1f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(2.290f, 3.02f, 2.4f), new Vector3(1f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(2.4f, 3.02f, 2.291f), new Vector3(1f, 20f, 0.05f), Vector3.up * -90f),

                    (new Vector3(3.71f, 0.55f, -2.292f), new Vector3(1f, 80f, 0.05f), Vector3.up * 90f),
                    (new Vector3(-2.292f, 0.55f, 3.71f), new Vector3(1f, 80f, 0.05f), Vector3.up * 0f),
                    (new Vector3(3.71f, 2.696f, -2.292f), new Vector3(1f, 20f, 0.05f), Vector3.up * 90f),
                    (new Vector3(-2.292f, 2.696f, 3.71f), new Vector3(1f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(3.71f, 3.02f, -2.292f), new Vector3(1f, 20f, 0.05f), Vector3.up * 90f),
                    (new Vector3(-2.292f, 3.02f, 3.71f), new Vector3(1f, 20f, 0.05f), Vector3.up * 0f)
                }
            },
            {
                RoomType.EzUpstairsPcs, new List<(Vector3, Vector3, Vector3)>()
                {
                    (new Vector3(0f, 0.55f, -5.89f), new Vector3(90f, 80f, 0.05f), Vector3.up * 90f),
                    (new Vector3(0f, 0.55f, 7.607f), new Vector3(90f, 80f, 0.05f), Vector3.up * -90f),
                    (new Vector3(0f, 2.696f, -5.89f), new Vector3(90f, 20f, 0.05f), Vector3.up * 90f),
                    (new Vector3(0f, 2.696f, 7.607f), new Vector3(90f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(0f, 3.02f, -5.89f), new Vector3(90f, 20f, 0.05f), Vector3.up * 90f),
                    (new Vector3(0f, 3.02f, 7.607f), new Vector3(90f, 20f, 0.05f), Vector3.up * -90f),

                    (new Vector3(10.154f, 0.55f, 4.427f), new Vector3(30f, 80f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-10.133f, 0.55f, 4.427f), new Vector3(30f, 80f, 0.05f), Vector3.up * 0f),
                    (new Vector3(10.154f, 2.696f, 4.427f), new Vector3(30f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-10.133f, 2.696f, 4.427f), new Vector3(30f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(10.154f, 3.02f, 4.427f), new Vector3(30f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-10.133f, 3.02f, 4.427f), new Vector3(30f, 20f, 0.05f), Vector3.up * 0f),

                    (new Vector3(10.154f, 0.55f, -3.51f), new Vector3(22f, 80f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-10.133f, 0.55f, -3.51f), new Vector3(22f, 80f, 0.05f), Vector3.up * 0f),
                    (new Vector3(10.154f, 2.696f, -3.51f), new Vector3(22f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-10.133f, 2.696f, -3.51f), new Vector3(22f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(10.154f, 3.02f, -3.51f), new Vector3(22f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-10.133f, 3.02f, -3.51f), new Vector3(22f, 20f, 0.05f), Vector3.up * 0f),

                    (new Vector3(-10.133f, 0.55f, 1.09f), new Vector3(1f, 80f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.133f, 0.55f, -1.09f), new Vector3(1f, 80f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.133f, 2.696f, 1.09f), new Vector3(1f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.133f, 2.696f, -1.09f), new Vector3(1f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.133f, 3.02f, 1.09f), new Vector3(1f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.133f, 3.02f, -1.09f), new Vector3(1f, 20f, 0.05f), Vector3.up * 0f),

                    (new Vector3(10.154f, 0.55f, 1.09f), new Vector3(1f, 80f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.154f, 0.55f, -1.09f), new Vector3(1f, 80f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.154f, 2.696f, 1.09f), new Vector3(1f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.154f, 2.696f, -1.09f), new Vector3(1f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.154f, 3.02f, 1.09f), new Vector3(1f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.154f, 3.02f, -1.09f), new Vector3(1f, 20f, 0.05f), Vector3.up * 180f)
                }
            },
            {
                RoomType.EzConference, new List<(Vector3, Vector3, Vector3)>()
                {
                    (new Vector3(10.142f, 0.55f, 1.64f), new Vector3(5.8f, 80f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.142f, 0.55f, -1.64f), new Vector3(5.8f, 80f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-10.142f, 0.55f, 1.64f), new Vector3(5.8f, 80f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.142f, 0.55f, -1.64f), new Vector3(5.8f, 80f, 0.05f), Vector3.up * 0f),

                    (new Vector3(10.142f, 2.696f, 1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.142f, 2.696f, -1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-10.142f, 2.696f, 1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.142f, 2.696f, -1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 0f),

                    (new Vector3(10.142f, 3.02f, 1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.142f, 3.02f, -1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-10.142f, 3.02f, 1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.142f, 3.02f, -1.64f), new Vector3(5.8f, 20f, 0.05f), Vector3.up * 0f),

                    (new Vector3(5.42f, 0.55f, -2.285f), new Vector3(43f, 80f, 0.05f), Vector3.up * 90f),
                    (new Vector3(6.41f, 0.55f, 2.291f), new Vector3(33f, 80f, 0.05f), Vector3.up * -90f),
                    (new Vector3(5.42f, 2.696f, -2.285f), new Vector3(43f, 20f, 0.05f), Vector3.up * 90f),
                    (new Vector3(6.41f, 2.696f, 2.291f), new Vector3(33f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(5.42f, 3.02f, -2.285f), new Vector3(43f, 20f, 0.05f), Vector3.up * 90f),
                    (new Vector3(6.41f, 3.02f, 2.291f), new Vector3(33f, 20f, 0.05f), Vector3.up * -90f),

                    (new Vector3(-5.95f, 0.55f, -2.285f), new Vector3(39f, 80f, 0.05f), Vector3.up * 90f),
                    (new Vector3(-6.41f, 0.55f, 2.291f), new Vector3(33f, 80f, 0.05f), Vector3.up * -90f),
                    (new Vector3(-5.95f, 2.696f, -2.285f), new Vector3(39f, 20f, 0.05f), Vector3.up * 90f),
                    (new Vector3(-6.41f, 2.696f, 2.291f), new Vector3(33f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(-5.95f, 3.02f, -2.285f), new Vector3(39f, 20f, 0.05f), Vector3.up * 90f),
                    (new Vector3(-6.41f, 3.02f, 2.291f), new Vector3(33f, 20f, 0.05f), Vector3.up * -90f),

                    (new Vector3(-2.62f, 0.55f, 3.325f), new Vector3(9f, 80f, 0.05f), Vector3.up * 0f),
                    (new Vector3(2.614f, 0.55f, 3.325f), new Vector3(9f, 80f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-2.62f, 2.696f, 3.325f), new Vector3(9f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(2.614f, 2.696f, 3.325f), new Vector3(9f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-2.62f, 3.02f, 3.325f), new Vector3(9f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(2.614f, 3.02f, 3.325f), new Vector3(9f, 20f, 0.05f), Vector3.up * 180f),

                    (new Vector3(1.893f, 0.55f, 4.26f), new Vector3(8f, 80f, 0.05f), Vector3.up * -90f),
                    (new Vector3(-1.903f, 0.55f, 4.26f), new Vector3(8f, 80f, 0.05f), Vector3.up * -90f),
                    (new Vector3(1.893f, 2.696f, 4.26f), new Vector3(8f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(-1.903f, 2.696f, 4.26f), new Vector3(8f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(1.893f, 3.02f, 4.26f), new Vector3(8f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(-1.903f, 3.02f, 4.26f), new Vector3(8f, 20f, 0.05f), Vector3.up * -90f),

                    (new Vector3(2.73f, 0.55f, 2.292f), new Vector3(1f, 80f, 0.05f), Vector3.up * -90f),
                    (new Vector3(-2.73f, 0.55f, 2.292f), new Vector3(1f, 80f, 0.05f), Vector3.up * -90f),
                    (new Vector3(2.73f, 2.696f, 2.292f), new Vector3(1f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(-2.73f, 2.696f, 2.292f), new Vector3(1f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(2.73f, 3.02f, 2.292f), new Vector3(1f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(-2.73f, 3.02f, 2.292f), new Vector3(1f, 20f, 0.05f), Vector3.up * -90f),

                    (new Vector3(0.6f, 0.55f, -2.286f), new Vector3(1f, 80f, 0.05f), Vector3.up * 90f),
                    (new Vector3(-1.59f, 0.55f, -2.286f), new Vector3(1f, 80f, 0.05f), Vector3.up * 90f),
                    (new Vector3(0.6f, 2.696f, -2.286f), new Vector3(1f, 20f, 0.05f), Vector3.up * 90f),
                    (new Vector3(-1.59f, 2.696f, -2.286f), new Vector3(1f, 20f, 0.05f), Vector3.up * 90f),
                    (new Vector3(0.6f, 3.02f, -2.286f), new Vector3(1f, 20f, 0.05f), Vector3.up * 90f),
                    (new Vector3(-1.59f, 3.02f, -2.286f), new Vector3(1f, 20f, 0.05f), Vector3.up * 90f)
                }
            },
            {
                RoomType.EzIntercom, new List<(Vector3, Vector3, Vector3)>()
                {
                    (new Vector3(3.4855f, 0.6f, 3.2f), new Vector3(12.05f, 87f, 0.05f), Vector3.up * 226.2f),
                    (new Vector3(3.4855f, 2.91f, 3.2f), new Vector3(12.05f, 20f, 0.05f), Vector3.up * 226.2f),
                    (new Vector3(3.4855f, 3.26f, 3.2f), new Vector3(12.05f, 20f, 0.05f), Vector3.up * 226.2f),

                    (new Vector3(2.574f, 0.6f, 4.0742f), new Vector3(1f, 87f, 0.05f), Vector3.up * 226.2f),
                    (new Vector3(4.41f, 0.6f, 2.314f), new Vector3(1f, 87f, 0.05f), Vector3.up * 226.2f),
                    (new Vector3(2.574f, 2.91f, 4.0742f), new Vector3(1f, 20f, 0.05f), Vector3.up * 226.2f),
                    (new Vector3(4.41f, 2.91f, 2.314f), new Vector3(1f, 20f, 0.05f), Vector3.up * 226.2f),
                    (new Vector3(2.574f, 3.26f, 4.0742f), new Vector3(1f, 20f, 0.05f), Vector3.up * 226.2f),
                    (new Vector3(4.41f, 3.26f, 2.314f), new Vector3(1f, 20f, 0.05f), Vector3.up * 226.2f),

                    (new Vector3(7.356f, 0.6f, 2.363f), new Vector3(25f, 87f, 0.05f), Vector3.up * -87.5f),
                    (new Vector3(4.6f, 0.6f, 2.247f), new Vector3(1f, 87f, 0.05f), Vector3.up * -87.5f),
                    (new Vector3(7.356f, 2.91f, 2.363f), new Vector3(25f, 20f, 0.05f), Vector3.up * -87.5f),
                    (new Vector3(4.6f, 2.91f, 2.247f), new Vector3(1f, 20f, 0.05f), Vector3.up * -87.5f),
                    (new Vector3(7.356f, 3.26f, 2.363f), new Vector3(25f, 20f, 0.05f), Vector3.up * -87.5f),
                    (new Vector3(4.6f, 3.26f, 2.247f), new Vector3(1f, 20f, 0.05f), Vector3.up * -87.5f),

                    (new Vector3(2.488f, 0.6f, 7.255f), new Vector3(27f, 87f, 0.05f), Vector3.up * 180f),
                    (new Vector3(2.488f, 0.6f, 4.27f), new Vector3(1f, 87f, 0.05f), Vector3.up * 180f),
                    (new Vector3(2.488f, 2.91f, 7.255f), new Vector3(27f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(2.488f, 2.91f, 4.27f), new Vector3(1f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(2.488f, 3.26f, 7.255f), new Vector3(27f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(2.488f, 3.26f, 4.27f), new Vector3(1f, 20f, 0.05f), Vector3.up * 180f),

                    (new Vector3(10.158f, 0.6f, 1.74f), new Vector3(6.65f, 87f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.158f, 0.6f, -1.775f), new Vector3(6.65f, 87f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.158f, 2.91f, 1.74f), new Vector3(6.65f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.158f, 2.91f, -1.775f), new Vector3(6.65f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.158f, 3.26f, 1.74f), new Vector3(6.65f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.158f, 3.26f, -1.775f), new Vector3(6.65f, 20f, 0.05f), Vector3.up * 180f),

                    (new Vector3(1.74f, 0.6f, 10.154f), new Vector3(6.65f, 87f, 0.05f), Vector3.up * -90f),
                    (new Vector3(-1.775f, 0.6f, 10.154f), new Vector3(6.65f, 87f, 0.05f), Vector3.up * -90f),
                    (new Vector3(1.74f, 2.91f, 10.154f), new Vector3(6.65f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(-1.775f, 2.91f, 10.154f), new Vector3(6.65f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(1.74f, 3.26f, 10.154f), new Vector3(6.65f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(-1.775f, 3.26f, 10.154f), new Vector3(6.65f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(-0.01f, 3.29f, 10.156f), new Vector3(8.75f, 12f, 0.05f), Vector3.up * -90f),
                    (new Vector3(10.158f, 3.29f, -0.01f), new Vector3(8.75f, 12f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-0.01f, 3.29f, -2.52f), new Vector3(8.8f, 10f, 0.05f), Vector3.up * 90f),

                    (new Vector3(6.9f, 0.6f, -2.518f), new Vector3(30f, 87f, 0.05f), Vector3.up * 90f),
                    (new Vector3(6.9f, 2.91f, -2.518f), new Vector3(30f, 20f, 0.05f), Vector3.up * 90f),
                    (new Vector3(6.9f, 3.26f, -2.518f), new Vector3(30f, 20f, 0.05f), Vector3.up * 90f),
                    (new Vector3(-2.514f, 0.6f, 4.5f), new Vector3(51f, 87f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-2.514f, 2.91f, 4.5f), new Vector3(51f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-2.514f, 3.26f, 4.5f), new Vector3(51f, 20f, 0.05f), Vector3.up * 0f),

                    (new Vector3(-2.274f, 0.6f, -1.258f), new Vector3(3.2f, 87f, 0.05f), Vector3.up * 45f),
                    (new Vector3(-2.274f, 2.91f, -1.258f), new Vector3(3.2f, 20f, 0.05f), Vector3.up * 45f),
                    (new Vector3(-2.274f, 3.26f, -1.258f), new Vector3(3.2f, 20f, 0.05f), Vector3.up * 45f),
                    (new Vector3(3.49f, 0.6f, -2.02f), new Vector3(6.25f, 87f, 0.05f), Vector3.up * 45f),
                    (new Vector3(3.49f, 2.91f, -2.02f), new Vector3(6.25f, 20f, 0.05f), Vector3.up * 45f),
                    (new Vector3(3.49f, 3.26f, -2.02f), new Vector3(6.25f, 20f, 0.05f), Vector3.up * 45f),

                    (new Vector3(-2.014f, 0.6f, -2.09f), new Vector3(5f, 87f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-2.014f, 2.91f, -2.09f), new Vector3(5f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-2.014f, 3.26f, -2.09f), new Vector3(5f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(1.99f, 0.6f, -2.09f), new Vector3(5f, 87f, 0.05f), Vector3.up * 180f),
                    (new Vector3(1.99f, 2.91f, -2.09f), new Vector3(5f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(1.99f, 3.26f, -2.09f), new Vector3(5f, 20f, 0.05f), Vector3.up * 180f),

                    (new Vector3(2.49f, 0.6f, -1.52f), new Vector3(4.4f, 87f, 0.05f), Vector3.up * 90f),
                    (new Vector3(2.49f, 2.91f, -1.52f), new Vector3(4.4f, 20f, 0.05f), Vector3.up * 90f),
                    (new Vector3(2.49f, 3.26f, -1.52f), new Vector3(4.4f, 20f, 0.05f), Vector3.up * 90f),

                    (new Vector3(-1.012f, 0.6f, -2.805f), new Vector3(2.5f, 87f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-1.012f, 2.91f, -2.805f), new Vector3(2.5f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-1.012f, 3.26f, -2.805f), new Vector3(2.5f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(0.988f, 0.6f, -2.805f), new Vector3(2.5f, 87f, 0.05f), Vector3.up * 180f),
                    (new Vector3(0.988f, 2.91f, -2.805f), new Vector3(2.5f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(0.988f, 3.26f, -2.805f), new Vector3(2.5f, 20f, 0.05f), Vector3.up * 180f),

                    (new Vector3(-1.515f, 0.6f, -2.518f), new Vector3(4.4f, 87f, 0.05f), Vector3.up * 90f),
                    (new Vector3(-1.515f, 2.91f, -2.518f), new Vector3(4.4f, 20f, 0.05f), Vector3.up * 90f),
                    (new Vector3(-1.515f, 3.26f, -2.518f), new Vector3(4.4f, 20f, 0.05f), Vector3.up * 90f),
                    (new Vector3(1.485f, 0.6f, -2.518f), new Vector3(4.4f, 87f, 0.05f), Vector3.up * 90f),
                    (new Vector3(1.485f, 2.91f, -2.518f), new Vector3(4.4f, 20f, 0.05f), Vector3.up * 90f),
                    (new Vector3(1.485f, 3.26f, -2.518f), new Vector3(4.4f, 20f, 0.05f), Vector3.up * 90f)
                }
            },
            {
                RoomType.EzDownstairsPcs, new List<(Vector3, Vector3, Vector3)>()
                {
                    (new Vector3(0f, 0.55f, 2.618f), new Vector3(90f, 80f, 0.05f), Vector3.up * -90f),
                    (new Vector3(0f, 2.696f, 2.618f), new Vector3(90f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(0f, 3.02f, 2.618f), new Vector3(90f, 20f, 0.05f), Vector3.up * -90f),
                    (new Vector3(0f, 0.55f, -10.143f), new Vector3(90f, 80f, 0.05f), Vector3.up * 90f),
                    (new Vector3(0f, 2.696f, -10.143f), new Vector3(90f, 20f, 0.05f), Vector3.up * 90f),
                    (new Vector3(0f, 3.02f, -10.143f), new Vector3(90f, 20f, 0.05f), Vector3.up * 90f),

                    (new Vector3(10.143f, 0.55f, -5.69f), new Vector3(41f, 80f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.143f, 2.696f, -5.69f), new Vector3(41f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.143f, 3.02f, -5.69f), new Vector3(41f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-10.143f, 0.55f, -5.69f), new Vector3(41f, 80f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.143f, 2.696f, -5.69f), new Vector3(41f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.143f, 3.02f, -5.69f), new Vector3(41f, 20f, 0.05f), Vector3.up * 0f),

                    (new Vector3(10.143f, 0.55f, 1.804f), new Vector3(7.2f, 80f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.143f, 2.696f, 1.804f), new Vector3(7.2f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.143f, 3.02f, 1.804f), new Vector3(7.2f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-10.143f, 0.55f, 1.804f), new Vector3(7.2f, 80f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.143f, 2.696f, 1.804f), new Vector3(7.2f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.143f, 3.02f, 1.804f), new Vector3(7.2f, 20f, 0.05f), Vector3.up * 0f),

                    (new Vector3(10.143f, 0.55f, 1.09f), new Vector3(1f, 80f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.143f, 2.696f, 1.09f), new Vector3(1f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.143f, 3.02f, 1.09f), new Vector3(1f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.143f, 0.55f, -1.09f), new Vector3(1f, 80f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.143f, 2.696f, -1.09f), new Vector3(1f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.143f, 3.02f, -1.09f), new Vector3(1f, 20f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-10.143f, 0.55f, 1.09f), new Vector3(1f, 80f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.143f, 2.696f, 1.09f), new Vector3(1f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.143f, 3.02f, 1.09f), new Vector3(1f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.143f, 0.55f, -1.09f), new Vector3(1f, 80f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.143f, 2.696f, -1.09f), new Vector3(1f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.143f, 3.02f, -1.09f), new Vector3(1f, 20f, 0.05f), Vector3.up * 0f),

                    (new Vector3(10.143f, -1.69f, -5.69f), new Vector3(41f, 40f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.143f, -0.62f, -5.69f), new Vector3(41f, 10f, 0.05f), Vector3.up * 180f),
                    (new Vector3(10.143f, -0.463f, -5.69f), new Vector3(41f, 10f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-10.143f, -1.69f, -5.69f), new Vector3(41f, 40f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.143f, -0.62f, -5.69f), new Vector3(41f, 10f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-10.143f, -0.463f, -5.69f), new Vector3(41f, 10f, 0.05f), Vector3.up * 0f),
                    (new Vector3(0f, -1.69f, -10.143f), new Vector3(90f, 40f, 0.05f), Vector3.up * 90f),
                    (new Vector3(0f, -0.62f, -10.143f), new Vector3(90f, 10f, 0.05f), Vector3.up * 90f),
                    (new Vector3(0f, -0.463f, -10.143f), new Vector3(90f, 10f, 0.05f), Vector3.up * 90f),

                    (new Vector3(0.16f, 0.63f, -1.31f), new Vector3(62f, 90f, 0.05f), Vector3.up * 90f),
                    (new Vector3(0.16f, 0.63f, -1.633f), new Vector3(62f, 90f, 0.05f), Vector3.up * -90f),
                    (new Vector3(0.16f, -0.11f, -1.633f), new Vector3(62f, 10f, 0.05f), Vector3.up * -90f),
                    (new Vector3(0.16f, -1.32f, -1.633f), new Vector3(62f, 47f, 0.05f), Vector3.up * -90f),
                    (new Vector3(6.98f, 0.63f, -1.31f), new Vector3(3f, 90f, 0.05f), Vector3.up * 90f),
                    (new Vector3(-6.652f, 0.63f, -1.31f), new Vector3(3f, 90f, 0.05f), Vector3.up * 90f),

                    (new Vector3(7.318f, 0.63f, -5.123f), new Vector3(1f, 90f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-6.993f, 0.63f, -5.123f), new Vector3(1f, 90f, 0.05f), Vector3.up * 180f),
                    (new Vector3(7.318f, 0.63f, -1.421f), new Vector3(1f, 90f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-6.993f, 0.63f, -1.421f), new Vector3(1f, 90f, 0.05f), Vector3.up * 180f),
                    (new Vector3(7.318f, -0.11f, -5.123f), new Vector3(1f, 10f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-6.993f, -0.11f, -5.123f), new Vector3(1f, 10f, 0.05f), Vector3.up * 180f),
                    (new Vector3(7.318f, -0.11f, -1.421f), new Vector3(1f, 10f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-6.993f, -0.11f, -1.421f), new Vector3(1f, 10f, 0.05f), Vector3.up * 180f),
                    (new Vector3(7.318f, -1.32f, -5.123f), new Vector3(1f, 47f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-6.993f, -1.32f, -5.123f), new Vector3(1f, 47f, 0.05f), Vector3.up * 180f),
                    (new Vector3(7.318f, -1.32f, -1.421f), new Vector3(1f, 47f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-6.993f, -1.32f, -1.421f), new Vector3(1f, 47f, 0.05f), Vector3.up * 180f),

                    (new Vector3(6.993f, 0.63f, -5.123f), new Vector3(1f, 90f, 0.05f), Vector3.up * 180f),
                    (new Vector3(6.993f, -0.11f, -5.123f), new Vector3(1f, 10f, 0.05f), Vector3.up * 180f),
                    (new Vector3(6.993f, -1.32f, -5.123f), new Vector3(1f, 47f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-6.666f, 0.63f, -5.123f), new Vector3(1f, 90f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-6.666f, -0.11f, -5.123f), new Vector3(1f, 10f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-6.666f, -1.32f, -5.123f), new Vector3(1f, 47f, 0.05f), Vector3.up * 0f),

                    (new Vector3(7.318f, 0.63f, -3.24f), new Vector3(16.5f, 90f, 0.05f), Vector3.up * 0f),
                    (new Vector3(7.318f, -0.11f, -3.24f), new Vector3(16.5f, 10f, 0.05f), Vector3.up * 0f),
                    (new Vector3(7.318f, -1.32f, -3.24f), new Vector3(16.5f, 47f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-6.993f, 0.63f, -3.24f), new Vector3(16.5f, 90f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-6.993f, -0.11f, -3.24f), new Vector3(16.5f, 10f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-6.993f, -1.32f, -3.24f), new Vector3(16.5f, 47f, 0.05f), Vector3.up * 180f),
                    (new Vector3(6.993f, 0.63f, -3.28f), new Vector3(16.5f, 90f, 0.05f), Vector3.up * 180f),
                    (new Vector3(6.993f, -0.11f, -3.28f), new Vector3(16.5f, 10f, 0.05f), Vector3.up * 180f),
                    (new Vector3(6.993f, -1.32f, -3.28f), new Vector3(16.5f, 47f, 0.05f), Vector3.up * 180f),
                    (new Vector3(-6.666f, 0.63f, -3.28f), new Vector3(16.5f, 90f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-6.666f, -0.11f, -3.28f), new Vector3(16.5f, 10f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-6.666f, -1.32f, -3.28f), new Vector3(16.5f, 47f, 0.05f), Vector3.up * 0f),

                    (new Vector3(7.155f, 0.63f, -5.234f), new Vector3(1.42f, 90f, 0.05f), Vector3.up * -90f),
                    (new Vector3(7.155f, -0.11f, -5.234f), new Vector3(1.42f, 10f, 0.05f), Vector3.up * -90f),
                    (new Vector3(7.155f, -1.32f, -5.234f), new Vector3(1.42f, 47f, 0.05f), Vector3.up * -90f),
                    (new Vector3(-6.829f, 0.63f, -5.234f), new Vector3(1.42f, 90f, 0.05f), Vector3.up * -90f),
                    (new Vector3(-6.829f, -0.11f, -5.234f), new Vector3(1.42f, 10f, 0.05f), Vector3.up * -90f),
                    (new Vector3(-6.829f, -1.32f, -5.234f), new Vector3(1.42f, 47f, 0.05f), Vector3.up * -90f)
                }
            },
            {
                RoomType.EzGateA, new List<(Vector3, Vector3, Vector3)>()
                {
                    #region Elevator-Right
                    (new Vector3(1.85f, 0.56f, 7.035f), new Vector3(21f, 81f, 0.05f), Vector3.up * 90f),//Elevator-Right-Bottom
                    (new Vector3(1.85f, 3.03f, 7.035f), new Vector3(21f, 18f, 0.05f), Vector3.up * 90f),//Elevator-Right-Top
                    (new Vector3(1.85f, 2.71f, 7.035f), new Vector3(21f, 18f, 0.05f), Vector3.up * 90f),//Elevator-Right-Middle

                    (new Vector3(4.186f, 0.56f, 7.035f), new Vector3(0.7f, 81f, 0.05f), Vector3.up * 90f),//Elevator-Right-Bottom-LeftEnding
                    (new Vector3(4.186f, 3.03f, 7.035f), new Vector3(0.7f, 18f, 0.05f), Vector3.up * 90f),//Elevator-Right-Top-LeftEnding
                    (new Vector3(4.186f, 2.71f, 7.035f), new Vector3(0.7f, 18f, 0.05f), Vector3.up * 90f),//Elevator-Right-Middle-LeftEnding
                    #endregion
                    #region Button
                    (new Vector3(-1.7673f, 0.56f, 5.3673f), new Vector3(20f, 81f, 0.05f), Vector3.up * 139.05f),//Button-Bottom
                    (new Vector3(-1.7673f, 3.03f, 5.3673f), new Vector3(20f, 18f, 0.05f), Vector3.up * 139.05f),//Button-Top
                    (new Vector3(-1.7673f, 2.71f, 5.3673f), new Vector3(20f, 18f, 0.05f), Vector3.up * 139.05f),//Button-Middle
                    #endregion
                    #region Elevator-Left
                    (new Vector3(8.625f, 0.56f, 7.035f), new Vector3(21f, 81f, 0.05f), Vector3.up * 90f),//Elevator-Left-Bottom
                    (new Vector3(8.625f, 3.03f, 7.035f), new Vector3(21f, 18f, 0.05f), Vector3.up * 90f),//Elevator-Left-Top
                    (new Vector3(8.625f, 2.71f, 7.035f), new Vector3(21f, 18f, 0.05f), Vector3.up * 90f),//Elevator-Left-Middle

                    (new Vector3(6.305f, 0.56f, 7.035f), new Vector3(0.7f, 81f, 0.05f), Vector3.up * 90f),//Elevator-Left-Bottom-RightEnding
                    (new Vector3(6.305f, 3.03f, 7.035f), new Vector3(0.7f, 18f, 0.05f), Vector3.up * 90f),//Elevator-Left-Top-RightEnding
                    (new Vector3(6.305f, 2.71f, 7.035f), new Vector3(0.7f, 18f, 0.05f), Vector3.up * 90f),//Elevator-Left-Middle-RightEnding
                    #endregion
                    #region Workbench
                    (new Vector3(-1.57f, 0.56f, -5.37f), new Vector3(14.95f, 81f, 0.05f), Vector3.up * -133.2f),//Workbench-Bottom
                    (new Vector3(-1.57f, 3.03f, -5.37f), new Vector3(14.95f, 18f, 0.05f), Vector3.up * -133.2f),//Workbench-Top
                    (new Vector3(-1.57f, 2.71f, -5.37f), new Vector3(14.95f, 18f, 0.05f), Vector3.up * -133.2f),//Workbench-Middle

                    (new Vector3(-0.395f, 0.56f, -6.47f), new Vector3(0.95f, 81f, 0.05f), Vector3.up * -133.2f),//Workbench-Bottom-RightEnd
                    (new Vector3(-0.395f, 3.03f, -6.47f), new Vector3(0.95f, 18f, 0.05f), Vector3.up * -133.2f),//Workbench-Top-RightEnd
                    (new Vector3(-0.395f, 2.71f, -6.47f), new Vector3(0.95f, 18f, 0.05f), Vector3.up * -133.2f),//Workbench-Middle-RightEnd
                    #endregion
                    #region Column-Left
                    (new Vector3(-0.32f, 0.56f, -6.955f), new Vector3(3.6f, 81f, 0.05f), Vector3.zero),//Column-Left-Left-Bottom
                    (new Vector3(-0.32f, 3.03f, -6.955f), new Vector3(3.6f, 18f, 0.05f), Vector3.zero),//Column-Left-Left-Top
                    (new Vector3(-0.32f, 2.71f, -6.955f), new Vector3(3.6f, 18f, 0.05f), Vector3.zero),//Column-Left-Left-Middle
                     
                    (new Vector3(0.994f, 0.56f, -6.955f), new Vector3(3.6f, 81f, 0.05f), Vector3.zero),//Column-Left-Right-Bottom
                    (new Vector3(0.994f, 3.03f, -6.955f), new Vector3(3.6f, 18f, 0.05f), Vector3.zero),//Column-Left-Right-Top
                    (new Vector3(0.994f, 2.71f, -6.955f), new Vector3(3.6f, 18f, 0.05f), Vector3.zero),//Column-Left-Right-Middle

                    (new Vector3(0.4f, 0.56f, -6.71f), new Vector3(10f, 81f, 0.05f), Vector3.up * 90f),//Column-Left-Middle-Bottom
                    (new Vector3(0.35f, 3.03f, -7.29f), new Vector3(6f, 18f, 0.05f), Vector3.up * 90f),//Column-Left-Middle-Top
                    (new Vector3(0.35f, 2.71f, -7.29f), new Vector3(6f, 18f, 0.05f), Vector3.up * 90f),//Column-Left-Middle-Middle
                    #endregion
                    #region Column-Middle
                    (new Vector3(-0.32f + 3.93f, 0.56f, -6.955f), new Vector3(3.6f, 81f, 0.05f), Vector3.zero),//Column-Middle-Left-Bottom
                    (new Vector3(-0.32f + 3.93f, 3.03f, -6.955f), new Vector3(3.6f, 18f, 0.05f), Vector3.zero),//Column-Middle-Left-Top
                    (new Vector3(-0.32f + 3.93f, 2.71f, -6.955f), new Vector3(3.6f, 18f, 0.05f), Vector3.zero),//Column-Middle-Left-Middle
                     
                    (new Vector3(0.994f + 3.92f, 0.56f, -6.955f), new Vector3(3.6f, 81f, 0.05f), Vector3.zero),//Column-Middle-Right-Bottom
                    (new Vector3(0.994f + 3.92f, 3.03f, -6.955f), new Vector3(3.6f, 18f, 0.05f), Vector3.zero),//Column-Middle-Right-Top
                    (new Vector3(0.994f + 3.92f, 2.71f, -6.955f), new Vector3(3.6f, 18f, 0.05f), Vector3.zero),//Column-Middle-Right-Middle

                    (new Vector3(0.4f + 3.93f, 0.56f, -6.71f), new Vector3(10f, 81f, 0.05f), Vector3.up * 90f),//Column-Middle-Middle-Bottom
                    (new Vector3(0.35f + 3.93f, 3.03f, -7.29f), new Vector3(6f, 18f, 0.05f), Vector3.up * 90f),//Column-Middle-Middle-Top
                    (new Vector3(0.35f + 3.93f, 2.71f, -7.29f), new Vector3(6f, 18f, 0.05f), Vector3.up * 90f),//Column-Middle-Middle-Middle
                    #endregion
                    #region Column-Right
                    (new Vector3(-0.32f + 3.93f + 3.93f, 0.56f, -6.955f), new Vector3(3.6f, 81f, 0.05f), Vector3.zero),//Column-Right-Left-Bottom
                    (new Vector3(-0.32f + 3.93f + 3.93f, 3.03f, -6.955f), new Vector3(3.6f, 18f, 0.05f), Vector3.zero),//Column-Right-Left-Top
                    (new Vector3(-0.32f + 3.93f + 3.93f, 2.71f, -6.955f), new Vector3(3.6f, 18f, 0.05f), Vector3.zero),//Column-Right-Left-Middle
                     
                    (new Vector3(0.994f + 3.92f + 3.93f, 0.56f, -6.955f), new Vector3(3.6f, 81f, 0.05f), Vector3.zero),//Column-Right-Right-Bottom
                    (new Vector3(0.994f + 3.92f + 3.93f, 3.03f, -6.955f), new Vector3(3.6f, 18f, 0.05f), Vector3.zero),//Column-Right-Right-Top
                    (new Vector3(0.994f + 3.92f + 3.93f, 2.71f, -6.955f), new Vector3(3.6f, 18f, 0.05f), Vector3.zero),//Column-Right-Right-Middle

                    (new Vector3(0.4f + 3.93f + 3.93f, 0.56f, -6.71f), new Vector3(10f, 81f, 0.05f), Vector3.up * 90f),//Column-Right-Middle-Bottom
                    (new Vector3(0.35f + 3.93f + 3.93f, 3.03f, -7.29f), new Vector3(6f, 18f, 0.05f), Vector3.up * 90f),//Column-Right-Middle-Top
                    (new Vector3(0.35f + 3.93f + 3.93f, 2.71f, -7.29f), new Vector3(6f, 18f, 0.05f), Vector3.up * 90f),//Column-Right-Middle-Middle
                    #endregion
                    #region Wall-Left
                    (new Vector3(2.3f, 0.56f, -6.54f), new Vector3(11.4f, 81f, 0.05f), Vector3.up * 90f),//Wall-Left-Bottom
                    (new Vector3(2.3f, 3.03f, -6.54f), new Vector3(11.4f, 18f, 0.05f), Vector3.up * 90f),//Wall-Left-Top
                    (new Vector3(2.3f, 2.71f, -6.54f), new Vector3(11.4f, 18f, 0.05f), Vector3.up * 90f),//Wall-Left-Middle

                    (new Vector3(3.5f, 0.56f, -6.54f), new Vector3(0.95f, 81f, 0.05f), Vector3.up * -90f),//Wall-Left-Bottom-RightEnd
                    (new Vector3(3.5f, 3.03f, -6.54f), new Vector3(0.95f, 18f, 0.05f), Vector3.up * -90f),//Wall-Left-Top-RightEnd
                    (new Vector3(3.5f, 2.71f, -6.54f), new Vector3(0.95f, 18f, 0.05f), Vector3.up * -90f),//Wall-Left-Middle-RightEnd

                    (new Vector3(1.1f, 0.56f, -6.54f), new Vector3(0.95f, 81f, 0.05f), Vector3.up * -90f),//Wall-Left-Bottom-LeftEnd
                    (new Vector3(1.1f, 3.03f, -6.54f), new Vector3(0.95f, 18f, 0.05f), Vector3.up * -90f),//Wall-Left-Top-LeftEnd
                    (new Vector3(1.1f, 2.71f, -6.54f), new Vector3(0.95f, 18f, 0.05f), Vector3.up * -90f),//Wall-Left-Middle-LeftEnd
                    #endregion
                    #region Wall-Middle
                    (new Vector3(6.225f, 0.56f, -6.54f), new Vector3(11.4f, 81f, 0.05f), Vector3.up * 90f),//Wall-Middle-Bottom
                    (new Vector3(6.225f, 3.03f, -6.54f), new Vector3(11.4f, 18f, 0.05f), Vector3.up * 90f),//Wall-Middle-Top
                    (new Vector3(6.225f, 2.71f, -6.54f), new Vector3(11.4f, 18f, 0.05f), Vector3.up * 90f),//Wall-Middle-Middle

                    (new Vector3(7.4275f, 0.56f, -6.54f), new Vector3(0.95f, 81f, 0.05f), Vector3.up * -90f),//Wall-Middle-Bottom-RightEnd
                    (new Vector3(7.4275f, 3.03f, -6.54f), new Vector3(0.95f, 18f, 0.05f), Vector3.up * -90f),//Wall-Middle-Top-RightEnd
                    (new Vector3(7.4275f, 2.71f, -6.54f), new Vector3(0.95f, 18f, 0.05f), Vector3.up * -90f),//Wall-Middle-Middle-RightEnd

                    (new Vector3(5.025f, 0.56f, -6.54f), new Vector3(0.95f, 81f, 0.05f), Vector3.up * -90f),//Wall-Middle-Bottom-LeftEnd
                    (new Vector3(5.025f, 3.03f, -6.54f), new Vector3(0.95f, 18f, 0.05f), Vector3.up * -90f),//Wall-Middle-Top-LeftEnd
                    (new Vector3(5.025f, 2.71f, -6.54f), new Vector3(0.95f, 18f, 0.05f), Vector3.up * -90f),//Wall-Middle-Middle-LeftEnd
                    #endregion
                    #region Wall-Right
                    (new Vector3(9.99f, 0.56f, -6.54f), new Vector3(10f, 81f, 0.05f), Vector3.up * 90f),//Wall-Right-Bottom
                    (new Vector3(9.99f, 3.03f, -6.54f), new Vector3(10f, 18f, 0.05f), Vector3.up * 90f),//Wall-Right-Top
                    (new Vector3(9.99f, 2.71f, -6.54f), new Vector3(10f, 18f, 0.05f), Vector3.up * 90f),//Wall-Right-Middle

                    (new Vector3(8.95f, 0.56f, -6.54f), new Vector3(0.95f, 81f, 0.05f), Vector3.up * -90f),//Wall-Right-Bottom-LeftEnd
                    (new Vector3(8.95f, 3.03f, -6.54f), new Vector3(0.95f, 18f, 0.05f), Vector3.up * -90f),//Wall-Right-Top-LeftEnd
                    (new Vector3(8.95f, 2.71f, -6.54f), new Vector3(0.95f, 18f, 0.05f), Vector3.up * -90f),//Wall-Right-Middle-LeftEnd
                    #endregion
                    #region BackWall-Left
                    (new Vector3(10.48f, 0.56f, -5.42f), new Vector3(10.2f, 81f, 0.05f), Vector3.zero),//BackWall-Left-Bottom
                    (new Vector3(10.48f, 3.03f, -5.42f), new Vector3(10.2f, 18f, 0.05f), Vector3.zero),//BackWall-Left-Top
                    (new Vector3(10.48f, 2.71f, -5.42f), new Vector3(10.2f, 18f, 0.05f), Vector3.zero),//BackWall-Left-Middle
                    #endregion
                    #region BackWall-Right
                    (new Vector3(10.48f, 0.56f, 5.745f), new Vector3(13f, 81f, 0.05f), Vector3.zero),//BackWall-Right-Bottom
                    (new Vector3(10.48f, 3.03f, 5.745f), new Vector3(13f, 18f, 0.05f), Vector3.zero),//BackWall-Right-Top
                    (new Vector3(10.48f, 2.71f, 5.745f), new Vector3(13f, 18f, 0.05f), Vector3.zero),//BackWall-Right-Middle
                    #endregion
                    #region BackWall-Middle
                    (new Vector3(10.59f, 0.56f, -4.254f), new Vector3(0.95f, 81f, 0.05f), Vector3.up * -90f),//BackWall-Middle-Left-Bottom
                    (new Vector3(10.59f, 3.03f, -4.254f), new Vector3(0.95f, 18f, 0.05f), Vector3.up * -90f),//BackWall-Middle-Left-Top
                    (new Vector3(10.59f, 2.71f, -4.254f), new Vector3(0.95f, 18f, 0.05f), Vector3.up * -90f),//BackWall-Middle-Left-Middle

                    (new Vector3(10.59f, 0.56f, 4.254f), new Vector3(0.95f, 81f, 0.05f), Vector3.up * 90f),//BackWall-Middle-Right-Bottom
                    (new Vector3(10.59f, 3.03f, 4.254f), new Vector3(0.95f, 18f, 0.05f), Vector3.up * 90f),//BackWall-Middle-Right-Top
                    (new Vector3(10.59f, 2.71f, 4.254f), new Vector3(0.95f, 18f, 0.05f), Vector3.up * 90f),//BackWall-Middle-Right-Middle
                    #endregion
                    #region Outside-Column-Left
                    (new Vector3(-0.32f - 7.685f, 0.56f, -6.955f + 2.295f), new Vector3(3.6f, 81f, 0.05f), Vector3.zero),// Outside-Column-Left-Left-Bottom
                    (new Vector3(-0.32f - 7.685f, 3.03f, -6.955f + 2.295f), new Vector3(3.6f, 18f, 0.05f), Vector3.zero),// Outside-Column-Left-Left-Top
                    (new Vector3(-0.32f - 7.685f, 2.71f, -6.955f + 2.295f), new Vector3(3.6f, 18f, 0.05f), Vector3.zero),// Outside-Column-Left-Left-Middle
                     
                    (new Vector3(0.994f - 7.69f, 0.56f, -6.955f + 2.295f), new Vector3(3.6f, 81f, 0.05f), Vector3.zero),// Outside-Column-Left-Right-Bottom
                    (new Vector3(0.994f - 7.69f, 3.03f, -6.955f + 2.295f), new Vector3(3.6f, 18f, 0.05f), Vector3.zero),// Outside-Column-Left-Right-Top
                    (new Vector3(0.994f - 7.69f, 2.71f, -6.955f + 2.295f), new Vector3(3.6f, 18f, 0.05f), Vector3.zero),// Outside-Column-Left-Right-Middle

                    (new Vector3(0.4f - 7.685f, 0.56f, -6.71f + 2.295f), new Vector3(10f, 81f, 0.05f), Vector3.up * 90f),// Outside-Column-Left-Middle-Bottom
                    (new Vector3(0.35f - 7.685f, 3.03f, -7.29f + 2.295f), new Vector3(6f, 18f, 0.05f), Vector3.up * 90f),//Outside-Column-Left-Middle-Top
                    (new Vector3(0.35f - 7.685f, 2.71f, -7.29f + 2.295f), new Vector3(6f, 18f, 0.05f), Vector3.up * 90f),//Outside-Column-Left-Middle-Bottom
                    #endregion
                    #region Outside-Column-Right
                    (new Vector3(-0.32f - 7.685f, 0.56f, -(-6.955f + 2.295f)), new Vector3(3.6f, 81f, 0.05f), Vector3.zero),// Outside-Column-Right-Left-Bottom
                    (new Vector3(-0.32f - 7.685f, 3.03f, -(-6.955f + 2.295f)), new Vector3(3.6f, 18f, 0.05f), Vector3.zero),// Outside-Column-Right-Left-Top
                    (new Vector3(-0.32f - 7.685f, 2.71f, -(-6.955f + 2.295f)), new Vector3(3.6f, 18f, 0.05f), Vector3.zero),// Outside-Column-Right-Left-Middle
                     
                    (new Vector3(0.994f - 7.69f, 0.56f, -(-6.955f + 2.295f)), new Vector3(3.6f, 81f, 0.05f), Vector3.zero),// Outside-Column-Right-Right-Bottom
                    (new Vector3(0.994f - 7.69f, 3.03f, -(-6.955f + 2.295f)), new Vector3(3.6f, 18f, 0.05f), Vector3.zero),// Outside-Column-Right-Right-Top
                    (new Vector3(0.994f - 7.69f, 2.71f, -(-6.955f + 2.295f)), new Vector3(3.6f, 18f, 0.05f), Vector3.zero),// Outside-Column-Right-Right-Middle

                    (new Vector3(0.4f - 7.685f, 0.56f, -(-6.71f + 2.295f)), new Vector3(10f, 81f, 0.05f), Vector3.up * 90f),// Outside-Column-Right-Middle-Bottom
                    (new Vector3(0.35f - 7.685f, 3.03f, -(-7.29f + 2.295f)), new Vector3(6f, 18f, 0.05f), Vector3.up * 90f),//Outside-Column-Right-Middle-Top
                    (new Vector3(0.35f - 7.685f, 2.71f, -(-7.29f + 2.295f)), new Vector3(6f, 18f, 0.05f), Vector3.up * 90f),//Outside-Column-Right-Middle-Bottom
                    #endregion
                    #region OutsideRightWall-Right
                    (new Vector3(-5.55f, 0.56f, -4.25f), new Vector3(10f, 81f, 0.05f), Vector3.up * -90f),//OutsideRightWall-Right-Bottom
                    (new Vector3(-5.55f, 3.03f, -4.25f), new Vector3(10f, 18f, 0.05f), Vector3.up * -90f),//OutsideRightWall-Right-Top
                    (new Vector3(-5.55f, 2.71f, -4.25f), new Vector3(10f, 18f, 0.05f), Vector3.up * -90f),//OutsideRightWall-Right-Middle
                    #endregion
                    #region OutsideRightWall-Left
                    (new Vector3(-9.15f, 0.56f, -4.25f), new Vector3(10f, 81f, 0.05f), Vector3.up * -90f),//OutsideRightWall-Left-Bottom
                    (new Vector3(-9.15f, 3.03f, -4.25f), new Vector3(10f, 18f, 0.05f), Vector3.up * -90f),//OutsideRightWall-Left-Top
                    (new Vector3(-9.15f, 2.71f, -4.25f), new Vector3(10f, 18f, 0.05f), Vector3.up * -90f),//OutsideRightWall-Left-Middle
                    #endregion
                    #region OutsideLeftWall-Right
                    (new Vector3(-5.55f, 0.56f, 4.25f), new Vector3(10f, 81f, 0.05f), Vector3.up * -90f),//OutsideLeftWall-Right-Bottom
                    (new Vector3(-5.55f, 3.03f, 4.25f), new Vector3(10f, 18f, 0.05f), Vector3.up * -90f),//OutsideLeftWall-Right-Top
                    (new Vector3(-5.55f, 2.71f, 4.25f), new Vector3(10f, 18f, 0.05f), Vector3.up * -90f),//OutsideLeftWall-Right-Middle
                    #endregion
                    #region OutsideLeftWall-Left
                    (new Vector3(-9.15f, 0.56f, 4.25f), new Vector3(10f, 81f, 0.05f), Vector3.up * -90f),//OutsideLeftWall-Left-Bottom
                    (new Vector3(-9.15f, 3.03f, 4.25f), new Vector3(10f, 18f, 0.05f), Vector3.up * -90f),//OutsideLeftWall-Left-Top
                    (new Vector3(-9.15f, 2.71f, 4.25f), new Vector3(10f, 18f, 0.05f), Vector3.up * -90f),//OutsideLeftWall-Left-Middle
                    #endregion
                    #region OutsideMiddleWall-Left
                    (new Vector3(-10.13f, 0.56f, 2.59f), new Vector3(14f, 81f, 0.05f), Vector3.zero),//OutsideMiddleWall-Left-Bottom
                    (new Vector3(-10.13f, 3.03f, 2.59f), new Vector3(14f, 18f, 0.05f), Vector3.zero),//OutsideMiddleWall-Left-Top
                    (new Vector3(-10.13f, 2.71f, 2.59f), new Vector3(14f, 18f, 0.05f), Vector3.zero),//OutsideMiddleWall-Left-Middle

                    (new Vector3(-10.13f, 0.56f, 1.09f), new Vector3(0.95f, 81f, 0.05f), Vector3.zero),//OutsideMiddleWall-Left-Bottom-RightEnding
                    (new Vector3(-10.13f, 3.03f, 1.09f), new Vector3(0.95f, 18f, 0.05f), Vector3.zero),//OutsideMiddleWall-Left-Top-RightEnding
                    (new Vector3(-10.13f, 2.71f, 1.09f), new Vector3(0.95f, 18f, 0.05f), Vector3.zero),//OutsideMiddleWall-Left-Middle-RightEnding
                    #endregion
                    #region OutsideMiddleWall-Right
                    (new Vector3(-10.13f, 0.56f, -2.59f), new Vector3(14f, 81f, 0.05f), Vector3.zero),//OutsideMiddleWall-Right-Bottom
                    (new Vector3(-10.13f, 3.03f, -2.59f), new Vector3(14f, 18f, 0.05f), Vector3.zero),//OutsideMiddleWall-Right-Top
                    (new Vector3(-10.13f, 2.71f, -2.59f), new Vector3(14f, 18f, 0.05f), Vector3.zero),//OutsideMiddleWall-Right-Middle

                    (new Vector3(-10.13f, 0.56f, -1.09f), new Vector3(0.95f, 81f, 0.05f), Vector3.zero),//OutsideMiddleWall-Right-Bottom-LeftEnding
                    (new Vector3(-10.13f, 3.03f, -1.09f), new Vector3(0.95f, 18f, 0.05f), Vector3.zero),//OutsideMiddleWall-Right-Top-LeftEnding
                    (new Vector3(-10.13f, 2.71f, -1.09f), new Vector3(0.95f, 18f, 0.05f), Vector3.zero),//OutsideMiddleWall-Right-Middle-LeftEnding
                    #endregion
                }
            },
            {
                //(new Vector3(-5.55f - 0.005f, 0.56f, -4.25f), new Vector3(10f, 81f, 0.05f), Vector3.up * -90f),//OutsideRightWall-Right-Bottom
                //(new Vector3(-5.55f - 0.005f, 3.03f, -4.25f), new Vector3(10f, 18f, 0.05f), Vector3.up * -90f),//OutsideRightWall-Right-Top
                //(new Vector3(-5.55f - 0.005f, 2.71f, -4.25f), new Vector3(10f, 18f, 0.05f), Vector3.up * -90f),//OutsideRightWall-Right-Middle
                RoomType.EzShelter, new List<(Vector3, Vector3, Vector3)>()
                {
                    #region Outside-Column-Left
                    (new Vector3(-8.06f, 0.56f, 4.573f), new Vector3(3.6f, 81f, 0.05f), Vector3.zero),//Outside-Column-Left-Right-Bottom
                    (new Vector3(-8.06f, 2.96f, 4.573f), new Vector3(3.6f, 18f, 0.05f), Vector3.zero),//Outside-Column-Left-Right-Top
                    (new Vector3(-8.06f, 2.64f, 4.573f), new Vector3(3.6f, 18f, 0.05f), Vector3.zero),//Outside-Column-Left-Right-Middle

                    (new Vector3(-6.78f, 0.56f, 4.573f), new Vector3(3.6f, 81f, 0.05f), Vector3.zero),//Outside-Column-Left-Left-Bottom
                    (new Vector3(-6.78f, 2.96f, 4.573f), new Vector3(3.6f, 18f, 0.05f), Vector3.zero),//Outside-Column-Left-Left-Top
                    (new Vector3(-6.78f, 2.64f, 4.573f), new Vector3(3.6f, 18f, 0.05f), Vector3.zero),//Outside-Column-Left-Left-Middle

                    (new Vector3(-7.425f, 0.56f, 4.32f), new Vector3(6f, 81f, 0.05f), Vector3.up * 90),//Outside-Column-Left-Middle-Bottom
                    (new Vector3(-7.425f, 2.96f, 4.89f), new Vector3(6f, 18f, 0.05f), Vector3.up * 90),//Outside-Column-Left-Middle-Top
                    (new Vector3(-7.425f, 2.64f, 4.89f), new Vector3(6f, 18f, 0.05f), Vector3.up * 90),//Outside-Column-Left-Middle-Middle
                    #endregion
                    #region Outside-Column-Right
                    //-7.575 0.56 -4.415 0 -90 0 6 81 0.05
                    (new Vector3(-8.06f-0.15f, 0.56f, -4.573f-0.095f), new Vector3(3.6f, 81f, 0.05f), Vector3.zero),//Outside-Column-Right-Right-Bottom
                    (new Vector3(-8.06f-0.15f, 3.03f, -4.573f-0.095f), new Vector3(3.6f, 18f, 0.05f), Vector3.zero),//Outside-Column-Right-Right-Top
                    (new Vector3(-8.06f-0.15f, 2.71f, -4.573f-0.095f), new Vector3(3.6f, 18f, 0.05f), Vector3.zero),//Outside-Column-Right-Right-Middle

                    (new Vector3(-6.78f-0.15f, 0.56f, -4.573f-0.095f), new Vector3(3.6f, 81f, 0.05f), Vector3.zero),//Outside-Column-Right-Left-Bottom
                    (new Vector3(-6.78f-0.15f, 3.03f, -4.573f-0.095f), new Vector3(3.6f, 18f, 0.05f), Vector3.zero),//Outside-Column-Right-Left-Top
                    (new Vector3(-6.78f-0.15f, 2.71f, -4.573f-0.095f), new Vector3(3.6f, 18f, 0.05f), Vector3.zero),//Outside-Column-Right-Left-Middle

                    (new Vector3(-7.425f-0.15f, 0.56f, -4.32f-0.095f), new Vector3(6f, 81f, 0.05f), Vector3.up * -90),//Outside-Column-Right-Middle-Bottom
                    (new Vector3(-7.425f-0.15f, 3.03f, -4.89f-0.095f), new Vector3(6f, 18f, 0.05f), Vector3.up * -90),//Outside-Column-Right-Middle-Top
                    (new Vector3(-7.425f-0.15f, 2.71f, -4.89f-0.095f), new Vector3(6f, 18f, 0.05f), Vector3.up * -90),//Outside-Column-Right-Middle-Middle
                    #endregion
                    #region OutsideRightWall-Right

                    #endregion
                    #region OutsideRightWall-Left
                    
                    #endregion
                    #region OutsideLeftWall-Right
                    
                    #endregion
                    #region OutsideLeftWall-Left
                    
                    #endregion
                    #region OutsideMiddleWall-Left
                    
                    #endregion
                    #region OutsideMiddleWall-Right
                    
                    #endregion
                }
            },
            {
                RoomType.HczEzCheckpoint, new List<(Vector3, Vector3, Vector3)>
                {
                    #region Column-Right
                    #region Bottom
                    (new Vector3(2.72f, 0.56f + 0.195f, -8.48f), new Vector3(3.6f, 81f, 0.05f), Vector3.up * 90f),//Column-Right-Bottom-Left-Bottom
                    (new Vector3(2.72f, 3.03f + 0.195f, -8.48f), new Vector3(3.6f, 18f, 0.05f), Vector3.up * 90f),//Column-Right-Bottom-Left-Top
                    (new Vector3(2.72f, 2.71f + 0.195f, -8.48f), new Vector3(3.6f, 18f, 0.05f), Vector3.up * 90f),//Column-Right-Bottom-Left-Middle
                     
                    (new Vector3(2.72f, 0.56f + 0.195f, -7.175f), new Vector3(3.6f, 81f, 0.05f), Vector3.up * 90f),//Column-Right-Bottom-Right-Bottom
                    (new Vector3(2.72f, 3.03f + 0.195f, -7.175f), new Vector3(3.6f, 18f, 0.05f), Vector3.up * 90f),//Column-Right-Bottom-Right-Top
                    (new Vector3(2.72f, 2.71f + 0.195f, -7.175f), new Vector3(3.6f, 18f, 0.05f), Vector3.up * 90f),//Column-Right-Bottom-Right-Middle
                    
                    (new Vector3(2.47f, 0.755f, -7.9f), new Vector3(10f, 81f, 0.05f), Vector3.zero),//Column-Right-Bottom-Bottom-Bottom
                    (new Vector3(3.055f, 2.31f, -7.9f), new Vector3(10f, 81f, 0.05f), Vector3.zero),//Column-Right-Bottom-Top-Bottom
                    #endregion
                    #region Top
                    (new Vector3(2.72f, 4.04f, -8.48f), new Vector3(3.6f, 81f, 0.05f), Vector3.up * 90f),//Column-Right-Top-Left-Bottom
                    (new Vector3(2.72f, 6.5f, -8.48f), new Vector3(3.6f, 18f, 0.05f), Vector3.up * 90f),//Column-Right-Top-Left-Top
                    (new Vector3(2.72f, 6.18f, -8.48f), new Vector3(3.6f, 18f, 0.05f), Vector3.up * 90f),//Column-Right-Top-Left-Middle
                     
                    (new Vector3(2.72f, 4.04f, -7.175f), new Vector3(3.6f, 81f, 0.05f), Vector3.up * 90f),//Column-Right-Top-Right-Bottom
                    (new Vector3(2.72f, 6.5f, -7.175f), new Vector3(3.6f, 18f, 0.05f), Vector3.up * 90f),//Column-Right-Top-Right-Top
                    (new Vector3(2.72f, 6.18f, -7.175f), new Vector3(3.6f, 18f, 0.05f), Vector3.up * 90f),//Column-Right-Top-Right-Middle
                    
                    (new Vector3(3.055f, 4.45f, -7.9f), new Vector3(10f, 18f, 0.05f), Vector3.zero),//Column-Right-Top-Bottom-Bottom
                    (new Vector3(3.055f, 4.77f, -7.9f), new Vector3(10f, 18f, 0.05f), Vector3.zero),//Column-Right-Top-Top-Bottom
                    (new Vector3(3.055f, 6.01f, -7.9f), new Vector3(10f, 81f, 0.05f), Vector3.zero),//Column-Right-Top-Middle-Bottom
                    #endregion
                    #endregion
                    #region Column-Left
                    #region Bottom
                    (new Vector3(-2.72f+0.0365f, 0.56f + 0.195f, -8.48f), new Vector3(3.6f, 81f, 0.05f), Vector3.up * 90f),//Column-Left-Bottom-Left-Bottom
                    (new Vector3(-2.72f+0.0365f, 3.03f + 0.195f, -8.48f), new Vector3(3.6f, 18f, 0.05f), Vector3.up * 90f),//Column-Left-Bottom-Left-Top
                    (new Vector3(-2.72f+0.0365f, 2.71f + 0.195f, -8.48f), new Vector3(3.6f, 18f, 0.05f), Vector3.up * 90f),//Column-Left-Bottom-Left-Middle
                     
                    (new Vector3(-2.72f+0.0365f, 0.56f + 0.195f, -7.175f), new Vector3(3.6f, 81f, 0.05f), Vector3.up * 90f),//Column-Left-Bottom-Right-Bottom
                    (new Vector3(-2.72f+0.0365f, 3.03f + 0.195f, -7.175f), new Vector3(3.6f, 18f, 0.05f), Vector3.up * 90f),//Column-Left-Bottom-Right-Top
                    (new Vector3(-2.72f+0.0365f, 2.71f + 0.195f, -7.175f), new Vector3(3.6f, 18f, 0.05f), Vector3.up * 90f),//Column-Left-Bottom-Right-Middle
                    
                    (new Vector3(-2.47f+0.0365f, 0.755f, -7.9f), new Vector3(10f, 81f, 0.05f), Vector3.zero),//Column-Left-Bottom-Bottom-Bottom
                    (new Vector3(-3.055f+0.0365f, 2.31f, -7.9f), new Vector3(10f, 81f, 0.05f), Vector3.zero),//Column-Left-Bottom-Top-Bottom
                    #endregion
                    #region Top
                    (new Vector3(-2.72f+0.0365f, 4.04f, -8.48f), new Vector3(3.6f, 81f, 0.05f), Vector3.up * 90f),//Column-Left-Top-Left-Bottom
                    (new Vector3(-2.72f+0.0365f, 6.5f, -8.48f), new Vector3(3.6f, 18f, 0.05f), Vector3.up * 90f),//Column-Left-Top-Left-Top
                    (new Vector3(-2.72f+0.0365f, 6.18f, -8.48f), new Vector3(3.6f, 18f, 0.05f), Vector3.up * 90f),//Column-Left-Top-Left-Middle
                     
                    (new Vector3(-2.72f+0.0365f, 4.04f, -7.175f), new Vector3(3.6f, 81f, 0.05f), Vector3.up * 90f),//Column-Left-Top-Right-Bottom
                    (new Vector3(-2.72f+0.0365f, 6.5f, -7.175f), new Vector3(3.6f, 18f, 0.05f), Vector3.up * 90f),//Column-Left-Top-Right-Top
                    (new Vector3(-2.72f+0.0365f, 6.18f, -7.175f), new Vector3(3.6f, 18f, 0.05f), Vector3.up * 90f),//Column-Left-Top-Right-Middle
                    
                    (new Vector3(-3.055f+0.0365f, 4.45f, -7.9f), new Vector3(10f, 18f, 0.05f), Vector3.zero),//Column-Left-Top-Bottom-Bottom
                    (new Vector3(-3.055f+0.0365f, 4.77f, -7.9f), new Vector3(10f, 18f, 0.05f), Vector3.zero),//Column-Left-Top-Top-Bottom
                    (new Vector3(-3.055f+0.0365f, 6.01f, -7.9f), new Vector3(10f, 81f, 0.05f), Vector3.zero),//Column-Left-Top-Middle-Bottom
                    #endregion
                    #endregion
                    #region Wall-Right
                    #region Bottom
                    (new Vector3(6.9f, 0.75f, -6.521f), new Vector3(40f, 81f, 0.05f), Vector3.up * -90f),//Wall-Right-Bottom-Bottom
                    (new Vector3(2.379f+0.0365f, 0.75f, -6.521f), new Vector3(0.95f, 81f, 0.05f), Vector3.up * -90f),//Wall-Right-Bottom-Bottom-EndRight

                    (new Vector3(6.9f, 2.9f, -6.521f), new Vector3(40f, 18f, 0.05f), Vector3.up * -90f),//Wall-Right-Bottom-Middle
                    (new Vector3(2.379f+0.0365f, 2.9f, -6.521f), new Vector3(0.95f, 18f, 0.05f), Vector3.up * -90f),//Wall-Right-Bottom-Middle-EndRight

                    (new Vector3(6.9f, 3.225f, -6.521f), new Vector3(40f, 18f, 0.05f), Vector3.up * -90f),//Wall-Right-Bottom-Top
                    (new Vector3(2.379f+0.0365f, 3.225f, -6.521f), new Vector3(0.95f, 18f, 0.05f), Vector3.up * -90f),//Wall-Right-Bottom-Top-EndRight
                    #endregion
                    #region Top
                    (new Vector3(6.3f+0.0365f, 4.03f, -6.521f), new Vector3(35f, 81f, 0.05f), Vector3.up * 90f),//Wall-Right-Top-Bottom
                    (new Vector3(2.379f+0.0365f, 4.03f, -6.521f), new Vector3(0.95f, 81f, 0.05f), Vector3.up * 90f),//Wall-Right-Top-Bottom-EndRight

                    (new Vector3(6.3f+0.0365f, 2.9f + 3.28f, -6.521f), new Vector3(35f, 18f, 0.05f), Vector3.up * 90f),//Wall-Right-Top-Middle
                    (new Vector3(2.379f+0.0365f, 2.9f + 3.28f, -6.521f), new Vector3(0.95f, 18f, 0.05f), Vector3.up * 90f),//Wall-Right-Top-Middle-EndRight

                    (new Vector3(6.3f+0.0365f, 3.225f + 3.28f, -6.521f), new Vector3(35f, 18f, 0.05f), Vector3.up * 90f),//Wall-Right-Top-Top
                    (new Vector3(2.379f+0.0365f, 3.225f + 3.28f, -6.521f), new Vector3(0.95f, 18f, 0.05f), Vector3.up * 90f),//Wall-Right-Top-Top-EndRight
                    #endregion
                    #endregion
                    #region Wall-Left
                    #region Bottom
                    (new Vector3(-6.3f, 0.75f, -6.521f), new Vector3(35f, 81f, 0.05f), Vector3.up * -90f),//Wall-Left-Bottom-Bottom
                    (new Vector3(-2.379f, 0.75f, -6.521f), new Vector3(0.95f, 81f, 0.05f), Vector3.up * -90f),//Wall-Left-Bottom-Bottom-EndRight

                    (new Vector3(-6.3f, 2.9f, -6.521f), new Vector3(35f, 18f, 0.05f), Vector3.up * -90f),//Wall-Left-Bottom-Middle
                    (new Vector3(-2.379f, 2.9f, -6.521f), new Vector3(0.95f, 18f, 0.05f), Vector3.up * -90f),//Wall-Left-Bottom-Middle-EndRight

                    (new Vector3(-6.3f, 3.225f, -6.521f), new Vector3(35f, 18f, 0.05f), Vector3.up * -90f),//Wall-Left-Bottom-Top
                    (new Vector3(-2.379f, 3.225f, -6.521f), new Vector3(0.95f, 18f, 0.05f), Vector3.up * -90f),//Wall-Left-Bottom-Top-EndRight
                    #endregion
                    #region Top
                    (new Vector3(-6.3f, 4.03f, -6.521f), new Vector3(35f, 81f, 0.05f), Vector3.up * 90f),//Wall-Left-Top-Bottom
                    (new Vector3(-2.379f, 4.03f, -6.521f), new Vector3(0.95f, 81f, 0.05f), Vector3.up * 90f),//Wall-Left-Top-Bottom-EndRight

                    (new Vector3(-6.3f, 2.9f + 3.28f, -6.521f), new Vector3(35f, 18f, 0.05f), Vector3.up * 90f),//Wall-Left-Top-Middle
                    (new Vector3(-2.379f, 2.9f + 3.28f, -6.521f), new Vector3(0.95f, 18f, 0.05f), Vector3.up * 90f),//Wall-Left-Top-Middle-EndRight

                    (new Vector3(-6.3f, 3.225f + 3.28f, -6.521f), new Vector3(35f, 18f, 0.05f), Vector3.up * 90f),//Wall-Left-Top-Top
                    (new Vector3(-2.379f, 3.225f + 3.28f, -6.521f), new Vector3(0.95f, 18f, 0.05f), Vector3.up * 90f),//Wall-Left-Top-Top-EndRight
                    #endregion
                    #endregion
                    #region Inside
                    (new Vector3(10.8f, 0.74f, 1f), new Vector3(70f, 81f, 0.05f), Vector3.zero),//Inside-Bottom
                    (new Vector3(10.8f, 3.215f, 1f), new Vector3(70f, 18f, 0.05f), Vector3.zero),//Inside-Top
                    (new Vector3(10.8f, 2.89f, 1f), new Vector3(70f, 18f, 0.05f), Vector3.zero),//Inside-Middle
                    #endregion
                    #region Middle
                    //+0.0365f
                    (new Vector3(0, 4.04f, -10.11f), new Vector3(22f, 81f, 0.05f), Vector3.up * 90f),//Middle-Top-Bottom
                    (new Vector3(0, 6.5f, -10.11f), new Vector3(22f, 18f, 0.05f), Vector3.up * 90f),//Middle-Top-Top
                    (new Vector3(0, 6.18f, -10.11f), new Vector3(22f, 18f, 0.05f), Vector3.up * 90f),//Middle-Top-Middle

                    (new Vector3(1.69f, 0.75f, -10.11f), new Vector3(6f, 81f, 0.05f), Vector3.up * -90f),//Middle-Bottom-Right-Bottom
                    (new Vector3(1.11f, 0.75f, -10.11f), new Vector3(0.95f, 81f, 0.05f), Vector3.up * -90f),//Middle-Bottom-Right-Bottom-LeftEnd
                    (new Vector3(1.69f, 3.03f + 0.195f, -10.11f), new Vector3(6f, 18f, 0.05f), Vector3.up * -90f),//Middle-Bottom-Right-Top
                    (new Vector3(1.11f, 3.03f + 0.195f, -10.11f), new Vector3(0.95f, 18f, 0.05f), Vector3.up * -90f),//Middle-Bottom-Right-Top-LeftEnd
                    (new Vector3(1.69f, 2.71f + 0.195f, -10.11f), new Vector3(6f, 18f, 0.05f), Vector3.up * -90f),//Middle-Bottom-Right-Middle
                    (new Vector3(1.11f, 2.71f + 0.195f, -10.11f), new Vector3(0.95f, 18f, 0.05f), Vector3.up * -90f),//Middle-Bottom-Right-Middle-LeftEnd

                    (new Vector3(-1.69f+0.0365f, 0.75f, -10.11f), new Vector3(6f, 81f, 0.05f), Vector3.up * -90f),//Middle-Bottom-Left-Bottom
                    (new Vector3(-1.11f+0.0380f, 0.75f, -10.11f), new Vector3(0.95f, 81f, 0.05f), Vector3.up * -90f),//Middle-Bottom-Left-Bottom-RightEnd
                    (new Vector3(-1.69f+0.0365f, 3.03f + 0.195f, -10.11f), new Vector3(6f, 18f, 0.05f), Vector3.up * -90f),//Middle-Bottom-Left-Top
                    (new Vector3(-1.11f+0.0380f, 3.03f + 0.195f, -10.11f), new Vector3(0.95f, 18f, 0.05f), Vector3.up * -90f),//Middle-Bottom-Left-Top-RightEnd
                    (new Vector3(-1.69f+0.0365f, 2.71f + 0.195f, -10.11f), new Vector3(6f, 18f, 0.05f), Vector3.up * -90f),//Middle-Bottom-Left-Middle
                    (new Vector3(-1.11f+0.0380f, 2.71f + 0.195f, -10.11f), new Vector3(0.95f, 18f, 0.05f), Vector3.up * -90f),//Middle-Bottom-Left-Middle-RightEnd
                    #endregion
                    #region Right
                    (new Vector3(-2.27f, 0.75f, -9.4f), new Vector3(8f, 81f, 0.05f), Vector3.zero),//Right-Left-Bottom-Bottom
                    (new Vector3(-2.27f, 3.03f + 0.195f, -9.4f), new Vector3(8f, 18f, 0.05f), Vector3.zero),//Right-Left-Bottom-Top
                    (new Vector3(-2.27f, 2.71f + 0.195f, -9.4f), new Vector3(8f, 18f, 0.05f), Vector3.zero),//Right-Left-Bottom-Middle

                    (new Vector3(-2.27f, 0.75f, -6.85f), new Vector3(2.9f, 81f, 0.05f), Vector3.zero),//Right-Right-Bottom-Bottom
                    (new Vector3(-2.27f, 3.03f + 0.195f, -6.85f), new Vector3(2.9f, 18f, 0.05f), Vector3.zero),//Right-Right-Bottom-Top
                    (new Vector3(-2.27f, 2.71f + 0.195f, -6.85f), new Vector3(2.9f, 18f, 0.05f), Vector3.zero),//Right-Right-Bottom-Middle

                    (new Vector3(-2.27f, 0.75f + 3.28f, -9.4f), new Vector3(8f, 81f, 0.05f), Vector3.zero),//Right-Left-Top-Bottom
                    (new Vector3(-2.27f, 3.03f + 0.195f + 3.28f, -9.4f), new Vector3(8f, 18f, 0.05f), Vector3.zero),//Right-Left-Top-Top
                    (new Vector3(-2.27f, 2.71f + 0.195f + 3.28f, -9.4f), new Vector3(8f, 18f, 0.05f), Vector3.zero),//Right-Left-Top-Middle

                    (new Vector3(-2.27f, 0.75f + 3.28f, -6.85f), new Vector3(2.9f, 81f, 0.05f), Vector3.zero),//Right-Right-Top-Bottom
                    (new Vector3(-2.27f, 3.03f + 0.195f + 3.28f, -6.85f), new Vector3(2.9f, 18f, 0.05f), Vector3.zero),//Right-Right-Top-Top
                    (new Vector3(-2.27f, 2.71f + 0.195f + 3.28f, -6.85f), new Vector3(2.9f, 18f, 0.05f), Vector3.zero),//Right-Right-Top-Middle
                    #endregion

                    #region Left
                    (new Vector3(2.27f+0.0365f, 0.75f, -9.4f), new Vector3(8f, 81f, 0.05f), Vector3.zero),//Left-Left-Bottom-Bottom
                    (new Vector3(2.27f+0.0365f, 3.03f + 0.195f, -9.4f), new Vector3(8f, 18f, 0.05f), Vector3.zero),//Left-Left-Bottom-Top
                    (new Vector3(2.27f+0.0365f, 2.71f + 0.195f, -9.4f), new Vector3(8f, 18f, 0.05f), Vector3.zero),//Left-Left-Bottom-Middle

                    (new Vector3(2.27f+0.0365f, 0.75f, -6.85f), new Vector3(2.9f, 81f, 0.05f), Vector3.zero),//Left-Right-Bottom-Bottom
                    (new Vector3(2.27f+0.0365f, 3.03f + 0.195f, -6.85f), new Vector3(2.9f, 18f, 0.05f), Vector3.zero),//Left-Right-Bottom-Top
                    (new Vector3(2.27f+0.0365f, 2.71f + 0.195f, -6.85f), new Vector3(2.9f, 18f, 0.05f), Vector3.zero),//Left-Right-Bottom-Middle

                    (new Vector3(2.27f+0.0365f, 0.75f + 3.28f, -9.4f), new Vector3(8f, 81f, 0.05f), Vector3.zero),//Left-Left-Top-Bottom
                    (new Vector3(2.27f+0.0365f, 3.03f + 0.195f + 3.28f, -9.4f), new Vector3(8f, 18f, 0.05f), Vector3.zero),//Left-Left-Top-Top
                    (new Vector3(2.27f+0.0365f, 2.71f + 0.195f + 3.28f, -9.4f), new Vector3(8f, 18f, 0.05f), Vector3.zero),//Left-Left-Top-Middle

                    (new Vector3(2.27f+0.0365f, 0.75f + 3.28f, -6.85f), new Vector3(2.9f, 81f, 0.05f), Vector3.zero),//Left-Right-Top-Bottom
                    (new Vector3(2.27f+0.0365f, 3.03f + 0.195f + 3.28f, -6.85f), new Vector3(2.9f, 18f, 0.05f), Vector3.zero),//Left-Right-Top-Top
                    (new Vector3(2.27f+0.0365f, 2.71f + 0.195f + 3.28f, -6.85f), new Vector3(2.9f, 18f, 0.05f), Vector3.zero),//Left-Right-Top-Middle
                    #endregion
                }
            }
        };
    }
}
