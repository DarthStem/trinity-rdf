﻿/*
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

Copyright (c) 2015 Semiodesk GmbH

Authors:
Moritz Eberl <moritz@semiodesk.com>
Sebastian Faubel <sebastian@semiodesk.com>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Collections.Specialized;
using System.Web;
using VDS.RDF.Query;

namespace Semiodesk.Trinity.Store
{
    /// <summary>
    /// Storage that can can connect to Sparql Endpoints 
    /// 
    /// </summary>
    /// <see cref="http://www.w3.org/TR/rdf-sparql-protocol/#SparqlQuery"/>
    internal class SparqlEndpointStorage : IStore
    {
        #region Members

        SparqlRemoteEndpoint _endpoint;

        public bool IsReady
        {
            get { throw new NotImplementedException(); }
        }
        #endregion

        #region Constructor
        public SparqlEndpointStorage(Uri endpointUri, IWebProxy proxy = null, ICredentials credentials = null)
        {
            _endpoint = new SparqlRemoteEndpoint(endpointUri);
            //_endpoint.Proxy = proxy;

            
        }
        #endregion

        #region Methods

        public IModel AddModel(Uri uri)
        {
            throw new NotSupportedException();
        }

        public void RemoveModel(Uri uri)
        {
            throw new NotSupportedException();
        }

        public void RemoveModel(IModel model)
        {
            throw new NotSupportedException();
        }

        public bool ContainsModel(Uri uri)
        {
            throw new NotSupportedException();
        }

        public bool ContainsModel(IModel model)
        {
            throw new NotSupportedException();
        }

        public IModel GetModel(Uri uri)
        {
            return new Model(this, uri.ToUriRef());
        }

        public IEnumerable<IModel> ListModels()
        {
            throw new NotSupportedException();
        }

        public ISparqlQueryResult ExecuteQuery(SparqlQuery query, ITransaction transaction = null)
        {

            SparqlResultSet result = _endpoint.QueryWithResultSet(query.ToString());
            return new SparqlEndpointQueryResult(result,  query);
        }


        public void ExecuteNonQuery(SparqlUpdate queryString, ITransaction transaction = null)
        {
            return;
        }

        public Uri Read(Uri graphUri, Uri url, RdfSerializationFormat format)
        {
            throw new NotSupportedException();
        }

        public System.IO.MemoryStream Write(Uri graphUri, RdfSerializationFormat format)
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
            return;
        }

        #endregion

        #region IStore Members


        public ITransaction BeginTransaction(System.Data.IsolationLevel isolationLevel)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
