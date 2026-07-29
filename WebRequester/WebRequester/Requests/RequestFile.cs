using System;
using System.IO;

namespace WebRequester.Requests
{
	public sealed class RequestFile
	{
		public string Name { get; set; }
		public string FileName { get; set; }
		public byte[] Data { get; set; }
		public string ContentType { get; set; }

		public static RequestFile FromPathAsFiles(string filePath, string contentType = null)
		{
			return FromPath("files", filePath, contentType);
		}

		public static RequestFile FromPath(string fieldName, string filePath, string contentType = null)
		{
			if (string.IsNullOrWhiteSpace(fieldName))
				throw new ArgumentException("Field name is required.", nameof(fieldName));
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentException("File path is required.", nameof(filePath));
			if (!File.Exists(filePath))
				throw new FileNotFoundException("File not found.", filePath);

			return new RequestFile
			{
				Name = fieldName,
				FileName = Path.GetFileName(filePath),
				Data = File.ReadAllBytes(filePath),
				ContentType = contentType
			};
		}

		public static RequestFile FromBytes(string fieldName, byte[] data, string fileName, string contentType = null)
		{
			if (string.IsNullOrWhiteSpace(fieldName))
				throw new ArgumentException("Field name is required.", nameof(fieldName));
			if (data == null || data.Length == 0)
				throw new ArgumentException("File data cannot be empty.", nameof(data));
			if (string.IsNullOrWhiteSpace(fileName))
				throw new ArgumentException("File name is required.", nameof(fileName));

			return new RequestFile
			{
				Name = fieldName,
				FileName = fileName,
				Data = data,
				ContentType = contentType
			};
		}

		public static RequestFile FromText(string fieldName, string text, string fileName = "file.txt", string contentType = "text/plain")
		{
			if (text == null)
				throw new ArgumentNullException(nameof(text));

			return FromBytes(fieldName, System.Text.Encoding.UTF8.GetBytes(text), fileName, contentType);
		}
	}
}
