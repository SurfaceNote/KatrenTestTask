using System;
using System.IO;
using System.Text;

namespace TestTask
{
    public class ReadOnlyStream : IReadOnlyStream, IDisposable
    {
        private bool _disposed;

        private Stream _localStream;

        /// <summary>
        /// Конструктор класса. 
        /// Т.к. происходит прямая работа с файлом, необходимо 
        /// обеспечить ГАРАНТИРОВАННОЕ закрытие файла после окончания работы с таковым!
        /// </summary>
        /// <param name="fileFullPath">Полный путь до файла для чтения</param>
        public ReadOnlyStream(string fileFullPath)
        {
            if (string.IsNullOrEmpty(fileFullPath) || !File.Exists(fileFullPath))
            {
                throw new ArgumentException("Неправильный путь или файла не существует");
            }

            _localStream = new FileStream(fileFullPath, FileMode.Open,FileAccess.Read);
            IsEof = _localStream.Length == 0;
            _disposed = false;
        }
                
        /// <summary>
        /// Флаг окончания файла.
        /// </summary>
        public bool IsEof
        {
            get;
            private set;
        }

        /// <summary>
        /// Ф-ция чтения следующего символа из потока.
        /// Если произведена попытка прочитать символ после достижения конца файла, метод 
        /// должен бросать соответствующее исключение
        /// </summary>
        /// <returns>Считанный символ.</returns>
        public char ReadNextChar()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ReadOnlyStream), "Стрим уже был Disposed");
            }

            if (_localStream == null)
            {
                throw new ObjectDisposedException(nameof(ReadOnlyStream), "Стрим не был инициализирован");
            }

            if (IsEof)
            {
                throw new EndOfStreamException("Попытка чтения запределом стрима");
            }

            try
            {
                byte[] buffer = new byte[4];
                int bytesRead = 0;
                int byteCount = 1;

                int firstByte = _localStream.ReadByte();
                buffer[bytesRead++] = (byte)firstByte;

                if (firstByte < 0x80)
                {
                    byteCount = 1;
                }
                else if ((firstByte & 0xE0) == 0xC0)
                {
                    byteCount = 2;
                }
                else if ((firstByte & 0xF0) == 0xE0)
                {
                    byteCount = 3;
                }    
                else if ((firstByte & 0xF8) == 0xF0)
                {
                    byteCount = 4;
                }

                while (bytesRead < byteCount)
                {
                    int nextByte = _localStream.ReadByte();
                    buffer[bytesRead++] = (byte)nextByte;
                }

                if (_localStream.Position == _localStream.Length)
                {
                    IsEof = true;
                }
                string charString = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                return charString[0];

            }
            catch(Exception ex)
            {
                throw new IOException("Во время чтения файла произошла ошибка");
            }

        }

        /// <summary>
        /// Сбрасывает текущую позицию потока на начало.
        /// </summary>
        public void ResetPositionToStart()
        {
            if (_localStream == null)
            {
                IsEof = true;
                return;
            }

            _localStream.Position = 0;
            IsEof = false;
        }

        public void Close()
        {
            Dispose(true);
            
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_localStream != null)
                {
                    try
                    {
                        _localStream.Close();
                        _localStream.Dispose();
                    }
                    catch (Exception ex)
                    {
                        // Можем добавит сюда обработку ошибки + логирование
                    }
                    _localStream = null;
                }
            }

            _disposed = true;
        }
    }
}
