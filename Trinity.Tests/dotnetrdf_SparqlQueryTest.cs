﻿// LICENSE:
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
// AUTHORS:
//
//  Moritz Eberl <moritz@semiodesk.com>
//  Sebastian Faubel <sebastian@semiodesk.com>
//
// Copyright (c) Semiodesk GmbH 2015

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;

using Semiodesk.Trinity;
using Semiodesk.Trinity.Ontologies;


using NUnit.Framework;

namespace Semiodesk.Trinity.Test
{
    [TestFixture]
    public class DotNetRDF_SparqlQueryTest
    {
        protected IStore Store;

        protected IModel Model = null;

        [SetUp]
        public void SetUp()
        {
            Store = StoreFactory.CreateStore("provider=dotnetrdf");
            Model = Store.GetModel(new Uri("http://example.org/TestModel"));

            if (!Model.IsEmpty)
            {
                Model.Clear();
            }

            OntologyDiscovery.AddNamespace("ex", new Uri("http://example.org/"));
            OntologyDiscovery.AddNamespace("dc", new Uri("http://purl.org/dc/elements/1.1/"));
            OntologyDiscovery.AddNamespace("vcard", new Uri("http://www.w3.org/2001/vcard-rdf/3.0#"));
            OntologyDiscovery.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));
            OntologyDiscovery.AddNamespace("dbpedia", new Uri("http://dbpedia.org/ontology/"));
            OntologyDiscovery.AddNamespace("dbpprop", new Uri("http://dbpedia.org/property/"));
            OntologyDiscovery.AddNamespace("schema", new Uri("http://schema.org/"));
            OntologyDiscovery.AddNamespace("nie", new Uri("http://www.semanticdesktop.org/ontologies/2007/01/19/nie#"));
            OntologyDiscovery.AddNamespace("nco", new Uri("http://www.semanticdesktop.org/ontologies/2007/03/22/nco#"));
            OntologyDiscovery.AddNamespace("test", new Uri("http://www.semiodesk.com/ontologies/test#"));
            OntologyDiscovery.AddNamespace("sfo", sfo.GetNamespace());
            OntologyDiscovery.AddNamespace(nfo.GetPrefix(), nfo.GetNamespace());

            //User u = UserManager.Instance.AddUser(new Uri("http://semiodesk.com/id/TestUser"));
            //u.GetDesktopFolder();
            //u.GetDownloadsFolder();

            Model.Clear();

            IResource resource0 = Model.CreateResource(new Uri("http://example.org/Hans"));
            resource0.AddProperty(rdf.type, nco.PersonContact);
            resource0.AddProperty(nco.fullname, "Hans Wurscht");
            resource0.AddProperty(nco.birthDate, DateTime.Now);
            resource0.AddProperty(nco.blogUrl, "http://blog.com/Hans");
            resource0.Commit();

            IResource resource1 = Model.CreateResource(new Uri("http://example.org/Task"));
            resource1.AddProperty(rdf.type, tmo.Task);
            resource1.AddProperty(tmo.taskName, "Eine Aufgabe.");
            resource1.AddProperty(nco.creator, resource0);
            resource1.Commit();

            IResource resource2 = Model.CreateResource(new Uri("http://example.org/Doc#1"));
            resource2.AddProperty(rdf.type, nfo.Document);
            resource2.AddProperty(dc.date, DateTime.Today);
            resource2.AddProperty(nco.creator, resource0);
            resource2.Commit();

            // NOTE: The different name influences the ordering of the resource in query results.
            IResource resource3 = Model.CreateResource(new Uri("http://example.org/Boc#2"));
            resource3.AddProperty(rdf.type, nfo.Document);
            resource3.AddProperty(dc.date, DateTime.Today.AddHours(1));
            resource3.AddProperty(nco.creator, resource0);
            resource3.Commit();

            IResource resource4 = Model.CreateResource(new Uri("http://example.org/Doc#3"));
            resource4.AddProperty(rdf.type, nfo.Document);
            resource4.AddProperty(dc.date, DateTime.Today.AddHours(2));
            resource4.AddProperty(nco.creator, resource0);
            resource4.Commit();
        }

        [TearDown]
        public void TearDown()
        {
            Model.Clear();
        }

        [Test]
        public void TestAsk()
        {
            // Checking the model using ASK queries.
            SparqlQuery query = new SparqlQuery("ASK { ?s nco:fullname 'Hans Wurscht' . }");
            ISparqlQueryResult result = Model.ExecuteQuery(query);

            Assert.AreEqual(true, result.GetAnwser());

            query = new SparqlQuery("ASK { ?s nco:fullname 'Hans Meier' . }");
            result = Model.ExecuteQuery(query);

            Assert.AreEqual(false, result.GetAnwser());
        }

        [Test]
        public void TestSelect()
        {
            // Retrieving bound variables using the SELECT query form.
            SparqlQuery query = new SparqlQuery("SELECT ?name ?birthday WHERE { ?x nco:fullname ?name. ?x nco:birthDate ?birthday. }");
            ISparqlQueryResult result = Model.ExecuteQuery(query);

            Assert.AreEqual(1, result.GetBindings().Count());

            // Retrieving resoures using the SELECT or DESCRIBE query form.
            query = new SparqlQuery("SELECT ?s ?p ?o WHERE { ?s ?p ?o. ?s nco:fullname 'Hans Wurscht'. }");
            result = Model.ExecuteQuery(query);

            Assert.AreEqual(1, result.GetResources().Count());

            // Test SELECT with custom defined PREFIXes
            query = new SparqlQuery("PREFIX nco: <http://www.semanticdesktop.org/ontologies/2007/03/22/nco#> SELECT ?s ?p ?o WHERE { ?s ?p ?o. ?s nco:fullname 'Hans Wurscht'. }");
            result = Model.ExecuteQuery(query);

            Assert.AreEqual(1, result.GetResources().Count());

            // Check if the select statement only works on the given model.
            query = new SparqlQuery("SELECT * WHERE { ?s ?p ?o. }");
            result = Model.ExecuteQuery(query);

            Assert.AreEqual(5, result.GetResources().Count());

            // Check that resource creation is done correctly for Resources containing dashes.
            IResource r0 = Model.CreateResource(new Uri("http://example.org/Something#0"));
            r0.AddProperty(new Property(new Uri("http://example.org/fullName")), "Something");
            r0.Commit();

            IResource r1 = Model.CreateResource(new Uri("http://example.org/Something#1"));
            r1.AddProperty(new Property(new Uri("http://example.org/fullName")), "Anotherthing");
            r1.Commit();

            query = new SparqlQuery("SELECT * WHERE { ?s ?p ?o. }");
            result = Model.ExecuteQuery(query);

            Assert.AreEqual(7, result.GetResources().Count());
        }

        [Test]
        public void TestDescribe()
        {
            SparqlQuery query = new SparqlQuery("DESCRIBE <http://example.org/Hans>");
            ISparqlQueryResult result = Model.ExecuteQuery(query);

            IList resources = result.GetResources().ToList();
            Assert.AreEqual(1, resources.Count);

            query = new SparqlQuery("DESCRIBE ?s WHERE { ?s nco:fullname 'Hans Wurscht'. }");
            result = Model.ExecuteQuery(query);

            resources = result.GetResources<PersonContact>().ToList();
            Assert.AreEqual(1, resources.Count);

            foreach (Contact c in resources)
            {
                Assert.AreEqual(c.GetType(), typeof(PersonContact));
            }
        }

        [Test]
        public void TestConstruct()
        {
            Assert.Inconclusive("Blank nodes are currently problematic.");
            SparqlQuery query = new SparqlQuery(@"
                CONSTRUCT
                {
                    ?x  vcard:N _:v .
                    _:v vcard:givenName ?name .
                }
                WHERE
                {
                    ?x nco:fullname ?name .
                }");

            ISparqlQueryResult result = Model.ExecuteQuery(query);

            IList resources = result.GetResources().ToList();
            Assert.AreEqual(2, resources.Count);
        }

        [Test]
        public void TestInferencing()
        {
            Assert.Inconclusive("dotnetrdf does not support inferencing.");
            Store = StoreFactory.CreateStore("provider=dotnetrdf;schema=Models/test-vocab.rdf");


            var model = Store.CreateModel(new Uri("http://example.org/TestModel"));

            Class horse = new Class(new Uri("http://www.semiodesk.com/ontologies/test#Horse"));
            Class animal = new Class(new Uri("http://www.semiodesk.com/ontologies/test#Animal"));
            Property eats = new Property(new Uri("http://www.semiodesk.com/ontologies/test#eats"));
            Property consumes = new Property(new Uri("http://www.semiodesk.com/ontologies/test#consumes"));

            IResource res = model.CreateResource(new Uri("http://www.example.org/Hans"));

            res.AddProperty(rdf.type, horse);
            res.AddProperty(eats, "Straw");
            res.Commit();


            SparqlQuery query;
            ISparqlQueryResult result;

            // This fact is not explicitly stated.
            query = new SparqlQuery("ASK WHERE { <http://www.example.org/Hans> a test:Animal . }");

            result = model.ExecuteQuery(query);
            Assert.IsFalse(result.GetAnwser());

            result = model.ExecuteQuery(query, true);
            Assert.IsTrue(result.GetAnwser());

            result = model.ExecuteQuery(query);
            Assert.IsFalse(result.GetAnwser());

            // This fact is not explicitly stated.
            query = new SparqlQuery("SELECT ?food WHERE { ?s test:consumes ?food . }");

            result = model.ExecuteQuery(query);
            Assert.AreEqual(0, result.GetBindings().Count());

            result = model.ExecuteQuery(query, true);
            Assert.AreEqual(1, result.GetBindings().Count());

        }

        [Test]
        public void TestModelApi()
        {
            // Retrieving resources using the model API.
            Assert.AreEqual(true, Model.ContainsResource(new Uri("http://example.org/Hans")));
            Assert.AreEqual(false, Model.ContainsResource(new Uri("http://example.org/Peter")));

            IResource hans = Model.GetResource(new Uri("http://example.org/Hans"));
            Assert.Throws(typeof(ArgumentException), delegate { Model.GetResource(new Uri("http://example.org/Peter")); });

            hans = Model.GetResource(new Uri("http://example.org/Hans"), typeof(Resource)) as IResource;
            Assert.NotNull(hans);
        }

        [Test]
        public void TestUriEscaping()
        {
            Uri uri = new Uri("file:///F:/test/02%20-%20Take%20Me%20Somewhere%20Nice.mp3");
            var x = Model.CreateResource(uri);
            var nameProperty = new Property(new Uri("ex:name"));
            x.AddProperty(nameProperty, "Name");
            x.Commit();

            var result = Model.GetResource(uri);
            Assert.AreEqual(x.Uri, result.Uri);
            Assert.AreEqual(x, result);
        }

        [Test]
        public void TestSelectCount()
        {
            SparqlQuery query = new SparqlQuery("SELECT COUNT(?s) AS ?count WHERE { ?s rdf:type nfo:Document. }");
            ISparqlQueryResult result = Model.ExecuteQuery(query);

            var bindings = result.GetBindings();
            Assert.AreEqual(1, bindings.Count());
            Assert.AreEqual(3, bindings.First()["count"]);
        }

        [Test]
        public void TestCount()
        {
            SparqlQuery query = new SparqlQuery("SELECT ?s ?p ?o WHERE { ?s rdf:type nfo:Document. ?s ?p ?o. }");
            ISparqlQueryResult result = Model.ExecuteQuery(query);

            Assert.AreEqual(3, result.Count());

            query = new SparqlQuery("SELECT ?s ?p ?o WHERE { ?s rdf:type nfo:Document. ?s ?p ?o. }");
            result = Model.ExecuteQuery(query);

            Assert.AreEqual(3, result.Count());
        }

        [Test]
        public void TestSetModel()
        {
            // Testing SetModel with a SparqlQuery that uses the query parser, this should succeed
            SparqlQuery query = new SparqlQuery("SELECT COUNT(?s) AS ?count WHERE { ?s ?p ?o . }");

            Assert.IsNull(query.Model);
            Assert.IsFalse(query.ToString().Contains("FROM"));

            query.Model = Model;

            Assert.NotNull(query.Model);
            Assert.IsTrue(query.ToString().Contains("FROM"));
        }

        [Test]
        public void TestComplexQuery()
        {
            string queryString = "SELECT ?s0 ?p0 ?o0 " +
                 "WHERE " +
                 "{{ " +
                    "?s0 ?p0 ?o0 . " +
                    "{{ " +
                     "  SELECT DISTINCT ?s0 " +
                       "WHERE " +
                       "{{ " +
                         " ?s ?p ?o." +
                          "?s <{0}> <{1}> ." +
                          "{{" +
                           "  ?s ?p1 ?o1 ." +
                             "FILTER ISLITERAL(?o1) . FILTER REGEX(STR(?o1), \"\", \"i\") ." +
                          "}}" +
                          "UNION" +
                          "{{" +
                             "?s ?p1 ?s1 ." +
                             "?s1 ?p2 ?o2 ." +
                             "FILTER ISLITERAL(?o2) . FILTER REGEX(STR(?o2), \"\", \"i\") ." +
                          "}}" +
                       "}}" +
                       "ORDER BY ?o" +
                    "}}" +
                 "}}";

            string q = string.Format(queryString, rdf.type.Uri.OriginalString, tmo.Task.Uri.OriginalString);
            SparqlQuery query = new SparqlQuery(q);

            MethodInfo method = query.GetType().GetMethod("SetLimit", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(query, new object[] { 10 });

            var x = Model.ExecuteQuery(query);
            var res = x.GetResources().ToList();
        }

        [Test]
        public void TestIsOrdered()
        {
            SparqlQuery query = new SparqlQuery(@"
                SELECT ?s0 ?p0 ?o0
                WHERE
                {
                    ?s0 ?p0 ?o0 .
                    {
                        SELECT DISTINCT ?s0
                        WHERE
                        {
                            ?s ?p ?o .
                            ?s @type @class .
                            {
                                ?s ?p1 ?o1 .
                                FILTER ISLITERAL(?o1) . FILTER REGEX(STR(?o1), '', 'i') .
                            }
                            UNION
                            {
                                ?s ?p1 ?s1 .
                                ?s1 ?p2 ?o2 .
                                FILTER ISLITERAL(?o2) . FILTER REGEX(STR(?o2), '', 'i') .
                            }
                        }
                        ORDER BY ?o
                    }
                }
            ");

            query.Bind("@type", rdf.type);
            query.Bind("@class", tmo.Task);

            Assert.IsTrue(string.IsNullOrEmpty(query.GetRootOrderByClause()));

            query = new SparqlQuery(@"
                SELECT ?s0 ?p0 ?o0
                WHERE
                {
                    ?s0 ?p0 ?o0 .
                    {
                        SELECT DISTINCT ?s0
                        WHERE
                        {
                            ?s ?p ?o .
                            ?s @type @class .
                            {
                                ?s ?p1 ?o1 .
                                FILTER ISLITERAL(?o1) . FILTER REGEX(STR(?o1), '', 'i') .
                            }
                            UNION
                            {
                                ?s ?p1 ?s1 .
                                ?s1 ?p2 ?o2 .
                                FILTER ISLITERAL(?o2) . FILTER REGEX(STR(?o2), '', 'i') .
                            }
                        }
                    }
                }
            ");

            query.Bind("@type", rdf.type);
            query.Bind("@class", tmo.Task);

            Assert.IsTrue(string.IsNullOrEmpty(query.GetRootOrderByClause()));

            query = new SparqlQuery(@"
                SELECT DISTINCT ?s0 FROM <http://semiodesk.com/id/8083cf10-5f90-40d4-b30a-c18fea31177b/>
                WHERE
                { 
                  ?s0 ?p0 ?o0 .
                  ?s0 a <http://www.semanticdesktop.org/ontologies/2007/03/22/nfo#Visual> . 
                  ?s0 <http://www.semanticdesktop.org/ontologies/2007/05/10/nexif#dateTime> ?o1 . 
                }
                ORDER BY ASC(?o1) LIMIT 50
            ");

            Assert.IsFalse(string.IsNullOrEmpty(query.GetRootOrderByClause()));
        }
    }
}
