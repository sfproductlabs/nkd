using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace NKD.Helpers
{
    public class DataReaderEnumerable : IEnumerable<IDataReader>, IDisposable
    {
        public IDataReader DataReader { get; private set; }
        private bool enumerated = false;

        public DataReaderEnumerable(IDataReader dataReader)
        {
            this.DataReader = dataReader;
        }

        #region IEnumerable<IDataReader> Members

        public IEnumerator<IDataReader> GetEnumerator()
        {
            if (enumerated)
                throw new InvalidOperationException("The IDataReader can only be enumerated once.");

            enumerated = true;
            return new DataReaderEnumerator(this.DataReader);
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this.DataReader.Dispose();
        }

        #endregion
    }

    public class DataReaderEnumerator : IEnumerator<IDataReader>
    {
        public IDataReader DataReader { get; private set; }

        public DataReaderEnumerator(IDataReader dataReader)
        {
            this.DataReader = dataReader;
        }

        #region IEnumerator<IDataReader> Members

        public IDataReader Current
        {
            get { return this.DataReader; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this.DataReader.Dispose();
        }

        #endregion

        #region IEnumerator Members

        object System.Collections.IEnumerator.Current
        {
            get { return this.Current; }
        }

        public bool MoveNext()
        {
            return this.DataReader.Read();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}