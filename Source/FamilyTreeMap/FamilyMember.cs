﻿using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace FamilyTree.FamilyTreeMap
{
    public class FamilyMember
    {
        public FamilyMember(Pawn pawn, int generation)
        {
            this.Pawn = pawn;
            Generation = generation;
        }
        
        private bool relationshipsCreated = false;
        
        public Pawn Pawn;
        
        private PawnNodeUnit nodeUnit;

        private Dictionary<FamilyMember, ProxyPawnNodeUnit> proxyNodes = new();

        private List<FamilyMember> loveInterests = new();

        public List<FamilyMember> parents = new();

        public List<FamilyMember> children = new();

        public bool IsAnchor = false;
        
        public int Generation;

        public void CreateLoveRelationships(List<FamilyMember> allNodes)
        {
            relationshipsCreated = true;
            
            allNodes.ForEach(node =>
            {
                node.Pawn.GetLoveRelations(true).ForEach(relationship =>
                {
                    if (relationship.otherPawn != Pawn) return;
                    if (!IsAnchor && !node.IsAnchor)
                    {
                        if (Generation != node.Generation)
                        {
                            IsAnchor = true;
                            node.IsAnchor = true;
                        }
                        
                        if (parents.Count == 0 && node.parents.Count > 0)
                            node.IsAnchor = true;
                        else
                            IsAnchor = true;
                    }
                        
                    loveInterests.Add(node);
                });
            });

            if (loveInterests.Count == 0) IsAnchor = true;
        }

        public void CreateParentRelationships(List<FamilyMember> allNodes)
        {
            allNodes.ForEach(node =>
            {
                node.Pawn.relations.DirectRelations.ForEach(relationship =>
                {
                    if (relationship.def.defName == "Parent" && relationship.otherPawn == Pawn)
                    {
                        node.parents.Add(this);
                        children.Add(node);
                    }
                });
            });
        }

        public bool HasLoveInterested() => loveInterests.Count > 0;

        public List<FamilyMember> GetLoveInterests() => loveInterests;

        public void AddedToNodeUnit(PawnNodeUnit unit) => nodeUnit = unit;

        public void AddProxyFor(FamilyMember familyMember, ProxyPawnNodeUnit proxyNodeUnit) =>
            proxyNodes[familyMember] = proxyNodeUnit;

        [CanBeNull]
        public PawnNodeUnit GetPawnNodeUnit() => nodeUnit;
        
        public bool IsParentProxy(FamilyMember parentToCheck)
        {
            var youngestGeneration = parents.Select(parent => parent.Generation).Min();

            return parentToCheck.Generation != youngestGeneration;
        }

        [CanBeNull]

        public PawnNodeUnit GetPawnNodeUnitFor(FamilyMember familyMember) =>
            proxyNodes.ContainsKey(familyMember) ? proxyNodes[familyMember] : GetPawnNodeUnit();
        
        [CanBeNull]
        public NodeUnit GetParentNodeUnit() => nodeUnit?.Parent;
    }
}