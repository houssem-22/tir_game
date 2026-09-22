using System;
using UnityEngine;

namespace Cipher.Clues
{
    public enum MatchPhase
    {
        Booting,
        Playing,
        Victory,
        Defeat,
        FailedGeneration
    }

    public sealed class MatchManager : MonoBehaviour
    {
        [SerializeField] int matchSeed = 847291;
        [SerializeField] float matchDurationSeconds = 15f * 60f;

        ClueDatabase _db;
        ClueMission _mission;
        ClueValidator _validator;
        ClueGenerator _generator;

        int _cluesFound;
        bool[] _found = new bool[3];
        float _timeRemaining;
        MatchPhase _phase = MatchPhase.Booting;
        string _status = "Initialisation…";

        public ClueDatabase Database => _db;
        public ClueMission Mission => _mission;
        public MatchPhase Phase => _phase;
        public float TimeRemaining => _timeRemaining;
        public int CluesFound => _cluesFound;
        public string StatusMessage => _status;
        public int Seed => matchSeed;
        public string ObjectiveText
        {
            get
            {
                if (_phase == MatchPhase.Victory) return "ACCESS GRANTED — VICTOIRE";
                if (_phase == MatchPhase.Defeat) return "TEMPS ÉCOULÉ — MISSION ÉCHOUÉE";
                if (_phase == MatchPhase.FailedGeneration) return _status;
                if (_mission == null) return "Génération des indices…";
                if (_cluesFound >= 3) return $"Atteignez {_mission.finalNodeId} et saisissez le code.";
                if (_mission.steps == null || _mission.steps.Length == 0) return "Aucun indice.";
                return _mission.steps[Mathf.Clamp(_cluesFound, 0, _mission.steps.Length - 1)].text;
            }
        }

        public event Action MissionReady;
        public event Action<int> ClueCollected;
        public event Action Victory;
        public event Action Changed;

        public bool TryGetObjectivePosition(out Vector3 worldPosition)
        {
            worldPosition = Vector3.zero;
            if (_db == null || _mission == null) return false;
            string id;
            if (_cluesFound >= 3)
            {
                id = "Bunker_Terminal";
            }
            else if (_mission.steps == null || _cluesFound < 0 || _cluesFound >= _mission.steps.Length)
            {
                return false;
            }
            else
            {
                id = _mission.steps[_cluesFound].targetNodeId;
            }

            var node = _db.Find(id);
            if (node == null) return false;
            worldPosition = node.worldPosition;
            return true;
        }

        public void Configure(ClueDatabase database, int seed)
        {
            _db = database;
            matchSeed = seed;
        }

        public void BeginMatch()
        {
            _validator = new ClueValidator();
            _generator = new ClueGenerator(_db, _validator);
            _mission = _generator.Generate(matchSeed);
            if (_mission == null || !_mission.validated)
            {
                var check = _validator.Validate(_mission, _db);
                if (!check.ok)
                {
                    _phase = MatchPhase.FailedGeneration;
                    _status = "Génération invalide — régénération…";
                    matchSeed += 17;
                    _mission = _generator.Generate(matchSeed);
                }
            }

            var finalCheck = _validator.Validate(_mission, _db);
            if (!finalCheck.ok)
            {
                _phase = MatchPhase.FailedGeneration;
                _status = finalCheck.message;
                Debug.LogError("[CIPHER] Match generation failed: " + finalCheck.message);
                Changed?.Invoke();
                return;
            }

            int stepCount = _mission.steps != null ? _mission.steps.Length : 3;
            _cluesFound = 0;
            _found = new bool[stepCount];
            _timeRemaining = matchDurationSeconds;
            _phase = MatchPhase.Playing;
            _status = $"Seed {_mission.seed} · 3 indices validés";
            Debug.Log("[CIPHER]\n" + _validator.Describe(_mission, _db));
            MissionReady?.Invoke();
            Changed?.Invoke();
        }

        void Update()
        {
            if (_phase != MatchPhase.Playing) return;
            _timeRemaining -= Time.deltaTime;
            if (_timeRemaining <= 0f)
            {
                _timeRemaining = 0f;
                _phase = MatchPhase.Defeat;
                _status = "Temps écoulé";
                Changed?.Invoke();
            }
        }

        public bool IsClueFound(int index)
        {
            if (index < 0 || index >= _found.Length) return false;
            return _found[index];
        }

        public bool TryCollectClue(string nodeId)
        {
            if (_phase != MatchPhase.Playing || _mission == null) return false;
            if (_cluesFound >= 3) return false;

            var expected = _mission.steps[_cluesFound];
            if (expected == null || expected.targetNodeId != nodeId)
            {
                _status = "Mauvais objet — suivez l'indice actuel.";
                Changed?.Invoke();
                return false;
            }

            _found[_cluesFound] = true;
            _cluesFound++;
            _status = _cluesFound >= 3
                ? $"Indices complets. Direction {_mission.finalNodeId}."
                : $"Indice {_cluesFound}/3 collecté.";
            ClueCollected?.Invoke(_cluesFound);
            Changed?.Invoke();
            return true;
        }

        public bool TrySubmitCode(string code)
        {
            if (_phase != MatchPhase.Playing || _mission == null) return false;
            if (_cluesFound < 3)
            {
                _status = "Code verrouillé — 3 indices requis.";
                Changed?.Invoke();
                return false;
            }

            if (string.Equals(code, _mission.code, StringComparison.Ordinal))
            {
                _phase = MatchPhase.Victory;
                _status = "ACCESS GRANTED";
                Victory?.Invoke();
                Changed?.Invoke();
                return true;
            }

            _status = "ACCESS DENIED";
            Changed?.Invoke();
            return false;
        }

        public string GetIntelLine(int index)
        {
            if (_mission == null || _mission.steps == null || index < 0 || index >= _mission.steps.Length)
            {
                return $"INDICE {index + 1}: ???";
            }

            if (!_found[index] && index > _cluesFound)
            {
                return $"INDICE {index + 1}: verrouillé";
            }

            string mark = _found[index] ? "[x]" : "[ ]";
            if (index == _cluesFound && !_found[index])
            {
                return $"{mark} INDICE {index + 1}: {_mission.steps[index].text}";
            }

            if (_found[index])
            {
                return $"{mark} INDICE {index + 1}: {_mission.steps[index].targetNodeId} OK";
            }

            return $"{mark} INDICE {index + 1}: ???";
        }
    }
}
