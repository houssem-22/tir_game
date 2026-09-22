using System.Text;

namespace Cipher.Clues
{
    public readonly struct ValidationResult
    {
        public readonly bool ok;
        public readonly string message;

        public ValidationResult(bool ok, string message)
        {
            this.ok = ok;
            this.message = message;
        }
    }

    /// <summary>
    /// Mandatory pre-match validation: existence, accessibility, coherence, code, reachability.
    /// </summary>
    public sealed class ClueValidator
    {
        public ValidationResult Validate(ClueMission mission, ClueDatabase db)
        {
            if (mission == null) return Fail("Mission null");
            if (db == null) return Fail("Database null");
            if (mission.steps == null || mission.steps.Length != 3) return Fail("Exactly 3 clues required");
            if (string.IsNullOrEmpty(mission.finalNodeId)) return Fail("Final node missing");
            if (string.IsNullOrEmpty(mission.code) || mission.code.Length != 4) return Fail("Code must be 4 digits");
            for (int i = 0; i < mission.code.Length; i++)
            {
                if (!char.IsDigit(mission.code[i])) return Fail("Code must be numeric");
            }

            var final = db.Find(mission.finalNodeId);
            if (final == null) return Fail($"Final node '{mission.finalNodeId}' not in map DB");
            if (!final.isFinalPoint && final.type != MapNodeType.Bunker)
            {
                return Fail($"Node '{mission.finalNodeId}' is not marked as final");
            }

            var seen = new System.Collections.Generic.HashSet<string>();
            for (int i = 0; i < mission.steps.Length; i++)
            {
                var step = mission.steps[i];
                if (step == null) return Fail($"Step {i + 1} null");
                if (string.IsNullOrEmpty(step.targetNodeId)) return Fail($"Step {i + 1} has no target");
                if (!seen.Add(step.targetNodeId)) return Fail($"Duplicate clue target {step.targetNodeId}");

                var node = db.Find(step.targetNodeId);
                if (node == null) return Fail($"Clue target '{step.targetNodeId}' not in map DB");
                if (!node.interactable) return Fail($"Clue target '{step.targetNodeId}' not interactable");
                if (string.IsNullOrEmpty(step.text)) return Fail($"Clue {i + 1} text empty");
            }

            // Coherence: each step (except last) should relate forward via relatedTargetId when available.
            for (int i = 0; i < mission.steps.Length - 1; i++)
            {
                var a = db.Find(mission.steps[i].targetNodeId);
                var b = db.Find(mission.steps[i + 1].targetNodeId);
                if (a == null || b == null) return Fail("Broken chain reference");
                if (!string.IsNullOrEmpty(a.relatedTargetId) && a.relatedTargetId != b.id)
                {
                    // Soft check: allow if b is in same building/zone as related target
                    var related = db.Find(a.relatedTargetId);
                    if (related == null) return Fail($"Related target '{a.relatedTargetId}' missing");
                    bool sameBuilding = !string.IsNullOrEmpty(related.buildingId) && related.buildingId == b.buildingId;
                    bool sameId = related.id == b.id;
                    if (!sameBuilding && !sameId)
                    {
                        return Fail($"Incoherent link {a.id} → {b.id} (expected {a.relatedTargetId})");
                    }
                }
            }

            var last = db.Find(mission.steps[2].targetNodeId);
            if (last != null && !string.IsNullOrEmpty(last.relatedTargetId))
            {
                if (last.relatedTargetId != mission.finalNodeId && last.id != mission.finalNodeId)
                {
                    var relatedFinal = db.Find(last.relatedTargetId);
                    if (relatedFinal == null || relatedFinal.id != mission.finalNodeId)
                    {
                        // Terminal_C → Bunker_07 is required truth for MVP1
                        if (last.relatedTargetId != mission.finalNodeId)
                        {
                            return Fail($"Clue 3 does not lead to final {mission.finalNodeId}");
                        }
                    }
                }
            }

            return new ValidationResult(true, "OK");
        }

        public string Describe(ClueMission mission, ClueDatabase db)
        {
            var sb = new StringBuilder();
            var result = Validate(mission, db);
            sb.AppendLine(result.ok ? "VALIDATION OK" : "VALIDATION FAIL: " + result.message);
            if (mission == null) return sb.ToString();
            sb.AppendLine($"Seed={mission.seed} Code={mission.code} Final={mission.finalNodeId}");
            if (mission.steps != null)
            {
                for (int i = 0; i < mission.steps.Length; i++)
                {
                    var s = mission.steps[i];
                    sb.AppendLine($"  [{s.index}] {s.targetNodeId}: {s.text}");
                }
            }
            return sb.ToString();
        }

        static ValidationResult Fail(string message) => new ValidationResult(false, message);
    }
}
