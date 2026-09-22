using System;
using System.Collections.Generic;

namespace Cipher.Clues
{
    /// <summary>
    /// RANDOM SEED → FINAL → clue3 → clue2 → clue1 → validate.
    /// Never invents nodes outside the database.
    /// </summary>
    public sealed class ClueGenerator
    {
        readonly ClueDatabase _db;
        readonly ClueValidator _validator;

        public ClueGenerator(ClueDatabase database, ClueValidator validator = null)
        {
            _db = database;
            _validator = validator ?? new ClueValidator();
        }

        public ClueMission Generate(int seed, int maxAttempts = 24)
        {
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                int attemptSeed = seed + attempt * 9973;
                var mission = BuildFromSeed(attemptSeed);
                var result = _validator.Validate(mission, _db);
                if (result.ok)
                {
                    mission.validated = true;
                    return mission;
                }
            }

            return BuildCanonical(seed);
        }

        ClueMission BuildFromSeed(int seed)
        {
            var rng = new System.Random(seed);
            var finals = _db.GetFinalPoints();
            if (finals.Count == 0) return BuildCanonical(seed);

            var finalPick = finals[rng.Next(finals.Count)];
            string finalId = ResolveFinalId(finalPick);

            MapNode clue1 = PickRelatedOrFallback(rng, new[] { "Camera_12", "Camera_03", "Computer_A1" }, "Camera_12");
            MapNode clue2 = _db.Find(clue1 != null ? clue1.relatedTargetId : null) ?? _db.Find("Building_04");
            MapNode clue3 = _db.Find(clue2 != null ? clue2.relatedTargetId : null) ?? _db.Find("Terminal_C");
            MapNode finalNode = _db.Find(finalId) ?? _db.Find("Bunker_07");

            if (clue1 == null || clue2 == null || clue3 == null || finalNode == null)
            {
                return BuildCanonical(seed);
            }

            return new ClueMission
            {
                seed = seed,
                finalNodeId = finalNode.id,
                code = GenerateCode(rng),
                steps = new[]
                {
                    MakeStep(1, clue1, BuildIntroText(clue1)),
                    MakeStep(2, clue2, BuildLinkText(clue1, clue2)),
                    MakeStep(3, clue3, BuildFinalRevealText(clue3, finalNode))
                }
            };
        }

        ClueMission BuildCanonical(int seed)
        {
            var rng = new System.Random(seed);
            var c1 = _db.Find("Camera_12");
            var c2 = _db.Find("Building_04");
            var c3 = _db.Find("Terminal_C");
            var finalNode = _db.Find("Bunker_07");
            if (c1 == null || c2 == null || c3 == null || finalNode == null)
            {
                return new ClueMission
                {
                    seed = seed,
                    validated = false,
                    steps = Array.Empty<ClueStep>()
                };
            }

            return new ClueMission
            {
                seed = seed,
                finalNodeId = finalNode.id,
                code = GenerateCode(rng),
                validated = true,
                steps = new[]
                {
                    MakeStep(1, c1, BuildIntroText(c1)),
                    MakeStep(2, c2, BuildLinkText(c1, c2)),
                    MakeStep(3, c3, BuildFinalRevealText(c3, finalNode))
                }
            };
        }

        string ResolveFinalId(MapNode finalPick)
        {
            if (finalPick.type == MapNodeType.Bunker) return finalPick.id;
            if (!string.IsNullOrEmpty(finalPick.relatedTargetId) && _db.Contains(finalPick.relatedTargetId))
            {
                return finalPick.relatedTargetId;
            }
            return finalPick.id;
        }

        MapNode PickRelatedOrFallback(System.Random rng, string[] candidates, string fallbackId)
        {
            var valid = new List<MapNode>();
            for (int i = 0; i < candidates.Length; i++)
            {
                var n = _db.Find(candidates[i]);
                if (n != null && n.isCluePoint && !string.IsNullOrEmpty(n.relatedTargetId) && _db.Contains(n.relatedTargetId))
                {
                    valid.Add(n);
                }
            }

            if (valid.Count == 0) return _db.Find(fallbackId);
            return valid[rng.Next(valid.Count)];
        }

        static ClueStep MakeStep(int index, MapNode current, string text)
        {
            return new ClueStep
            {
                index = index,
                targetNodeId = current.id,
                text = text,
                nextHint = string.Empty
            };
        }

        static string BuildIntroText(MapNode clue1)
        {
            return ClueTemplates.Format(ClueTemplates.SearchObjectInZone, new Dictionary<string, string>
            {
                { "OBJECT", clue1.displayName },
                { "ZONE", ClueTemplates.ZoneDisplay(clue1) }
            });
        }

        static string BuildLinkText(MapNode from, MapNode to)
        {
            if (from != null && from.type == MapNodeType.Camera)
            {
                return ClueTemplates.Format(ClueTemplates.CameraPointsToBuilding, new Dictionary<string, string>
                {
                    { "OBJECT", from.displayName },
                    { "DIRECTION", string.IsNullOrEmpty(from.facesDirection) ? "East" : from.facesDirection },
                    { "TARGET", to.displayName }
                });
            }

            return ClueTemplates.Format(ClueTemplates.BuildingLeadsToTerminal, new Dictionary<string, string>
            {
                { "OBJECT", from != null ? from.displayName : to.displayName },
                { "ROOM", to.roomId ?? "Room" },
                { "TARGET", to.displayName }
            });
        }

        static string BuildFinalRevealText(MapNode clue3, MapNode finalNode)
        {
            return ClueTemplates.Format(ClueTemplates.TerminalRevealsFinal, new Dictionary<string, string>
            {
                { "OBJECT", clue3.displayName },
                { "FINAL", finalNode.displayName }
            });
        }

        static string GenerateCode(System.Random rng)
        {
            return $"{rng.Next(0, 10)}{rng.Next(0, 10)}{rng.Next(0, 10)}{rng.Next(0, 10)}";
        }
    }
}
