using System;
using System.IO;
using System.Text;

// This is a subset of the full Palaso.IO.TempFile

namespace NetSparkle
{
    /// <summary>
	/// This is useful a temporary file is needed. When it is disposed, it will delete the file.
	/// </summary>
	/// <example>using(f = new TempFile())</example>
	public class TempFile : IDisposable
	{
		private string _path;

		internal TempFile()
		{
			 _path = System.IO.Path.GetTempFileName();
		}
		
		internal TempFile(bool dontMakeMeAFileAndDontSetPath)
		{
            if(!dontMakeMeAFileAndDontSetPath)
            {
                _path = System.IO.Path.GetTempFileName();
            }
		}

		internal TempFile(string contents)
			: this()
		{
			File.WriteAllText(_path, contents);
		}

        internal TempFile(string contents, Encoding encoding)
            : this()
        {
            File.WriteAllText(_path, contents, encoding);
        }

        internal TempFile(string[] contentLines)
			: this()
		{
			File.WriteAllLines(_path, contentLines);
		}

		internal string Path
		{
			get { return _path; }
		}

        /// <summary>
        /// 
        /// </summary>
	    public void Dispose()
		{
			File.Delete(_path);
		}

		internal static TempFile CopyOf(string pathToExistingFile)
		{
			TempFile t = new TempFile();
			File.Copy(pathToExistingFile, t.Path, true);
			return t;
		}

		internal TempFile(string existingPath, bool dummy)
		{
			_path = existingPath;
		}

		/// <summary>
		/// Create a TempFile based on a pre-existing file, which will be deleted when this is disposed.
		/// </summary>
		internal static TempFile TrackExisting(string path)
		{
			return new TempFile(path, false);
		}

		internal static TempFile CreateAndGetPathButDontMakeTheFile()
		{
			TempFile t = new TempFile();
			File.Delete(t.Path);
			return t;
		}

		/// <summary>
		/// Use this one when it's important to have a certain file extension 
		/// </summary>
		/// <param name="extension">with or with out '.', will work the same</param>
		internal static TempFile WithExtension(string extension)
		{
			extension = extension.TrimStart('.');
			var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName() + "." + extension);
			File.Create(path).Close();
			return TrackExisting(path);
		}
	}
}
