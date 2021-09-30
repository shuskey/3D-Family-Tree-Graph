﻿using Assets.Scripts.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Data.Sqlite;
using System.Data;
using UnityEngine;
using Assets.Scripts.Enums;

namespace Assets.Scripts.DataProviders
{
    class ListOfParentsFromDataBase
    {
        public List<Parentage> parentList;
        private string _dataBaseFileName;

        public ListOfParentsFromDataBase(string DataBaseFileName)           
        {
            _dataBaseFileName = DataBaseFileName;
            parentList = new List<Parentage>();
        }

        public void GetListOfParentsFromDataBase(int childID)
        {
            string conn = "URI=file:" + _dataBaseFileName;

            IDbConnection dbconn;
            dbconn = (IDbConnection)new SqliteConnection(conn);
            dbconn.Open();
            IDbCommand dbcmd = dbconn.CreateCommand();
            dbcmd.CommandText =
                "SELECT family.FamilyID, family.FatherID, family.MotherID, children.ChildID, children.RelFather, children.RelMother \n" +
                "FROM FamilyTable family \n" +
                "JOIN NameTable father \n" +
                "   ON family.FatherID = father.OwnerID \n" +
                "JOIN NameTable mother \n" +
                "   ON family.MotherID = mother.OwnerID \n" +
                "JOIN ChildTable children \n" +
                "   ON family.FamilyID = children.FamilyID \n" +
                "   JOIN NameTable child \n" +
                "      ON children.ChildID = child.OwnerID \n" +
                $"WHERE children.ChildID = \"{ childID}\";";

            IDataReader reader = dbcmd.ExecuteReader();            
            while (reader.Read())
            {
                var parantage = new Parentage(
                    familyId: reader.GetInt32(0),
                    fatherId: reader.GetInt32(1),
                    motherId: reader.GetInt32(2),
                    childId: reader.GetInt32(3),
                    relationToFather: reader.GetInt32(4) == 0 ? ChildRelationshipType.Biological : ChildRelationshipType.Adopted,
                    relationToMother: reader.GetInt32(5) == 0 ? ChildRelationshipType.Biological : ChildRelationshipType.Adopted);

                parentList.Add(parantage);                
            }
            reader.Close();
            reader = null;
            dbcmd.Dispose();
            dbcmd = null;
            dbconn.Close();
            dbconn = null;
        }
    }        
}
