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

                    (new Vector3(-2f, 0.6f, -2.09f), new Vector3(5f, 87f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-2f, 2.91f, -2.09f), new Vector3(5f, 20f, 0.05f), Vector3.up * 0f),
                    (new Vector3(-2f, 3.26f, -2.09f), new Vector3(5f, 20f, 0.05f), Vector3.up * 0f),
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
                    (new Vector3(0.988f, 3.26f, -2.805f), new Vector3(2.5f, 20f, 0.05f), Vector3.up * 180f)
                }
            }
        };
    }
}
