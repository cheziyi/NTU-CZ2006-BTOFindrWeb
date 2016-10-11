﻿using BTOFindr.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BTOFindrWeb.Controllers
{
    public class ProjectController : ApiController
    {
        String connString = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;


        [HttpGet]
        public List<string> GetTownNames()
        {
            List<string> towns = new List<string>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    String query = "SELECT DISTINCT TownName FROM Projects ORDER BY TownName ASC";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                towns.Add(dr["TownName"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return towns;
        }


        [HttpGet]
        public Project GetProject(string projectId)
        {
            Project project = new Project();

            if (projectId == null)
            {
                return project;
            }
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    String query = "SELECT * FROM Projects WHERE ProjectId = @ProjectId";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProjectId", projectId);

                        conn.Open();

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                project.projectId = dr["ProjectId"].ToString();
                                project.projectName = dr["ProjectName"].ToString();
                                project.townName = dr["TownName"].ToString();
                                project.ballotDate = dr["BallotDate"].ToString();
                                project.projectImage = dr["ProjectImage"].ToString();

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return project;
        }

        [HttpPost]
        public string AddProject(Project project)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    String query = "SELECT * FROM Projects WHERE ProjectName=@ProjectName";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProjectName", project.projectName);

                        conn.Open();

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                               return dr["ProjectId"].ToString();
                            }
                        }
                    }
                }

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    String query = "INSERT INTO Projects(ProjectId,ProjectName,TownName,BallotDate,ProjectImage) VALUES(@ProjectId,@ProjectName,@TownName,@BallotDate,@ProjectImage)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProjectId", project.projectId);
                        cmd.Parameters.AddWithValue("@ProjectName", project.projectName);
                        cmd.Parameters.AddWithValue("@TownName", project.townName);
                        cmd.Parameters.AddWithValue("@BallotDate", project.ballotDate);
                        cmd.Parameters.AddWithValue("@ProjectImage", project.projectImage);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                    using (MessagingController mc = new MessagingController())
                        mc.SendFCMMessage("projects", "New BTO Project '" + project.projectName + "' released!");

                    return project.projectId;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
