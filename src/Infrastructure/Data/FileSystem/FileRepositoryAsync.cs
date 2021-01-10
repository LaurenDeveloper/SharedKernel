﻿using SharedKernel.Application.UnitOfWorks;
using SharedKernel.Domain.Entities;
using SharedKernel.Domain.Repositories;
using SharedKernel.Infrastructure.Data.FileSystem.UnitOfWorks;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SharedKernel.Infrastructure.Data.FileSystem
{
    /// <summary>
    /// 
    /// </summary>
    public class FileRepositoryAsync : IFileRepositoryAsync
    {
        private readonly FileSystemUnitOfWork _unitOfWork;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitOfWork"></param>
        public FileRepositoryAsync(IFileSystemUnitOfWorkAsync unitOfWork)
        {
            _unitOfWork = unitOfWork as FileSystemUnitOfWork;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns></returns>
        public Task<string[]> ReadAllLinesAsync(string path, CancellationToken cancellationToken)
        {
#if NETSTANDARD2_1
            return File.ReadAllLinesAsync(path, cancellationToken);
#else
            return Task.FromResult(File.ReadAllLines(path));
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns></returns>
        public Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken)
        {
            var file = _unitOfWork.Files.SingleOrDefault(x => x.Id == path);
            if (file != null)
                return Task.FromResult(file.Contents);

#if NETSTANDARD2_1
            return File.ReadAllBytesAsync(path, cancellationToken);
#else
            return Task.FromResult(File.ReadAllBytes(path));
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns></returns>
        public Task<bool> ExistsAsync(string id, CancellationToken cancellationToken)
        {
            return Task.FromResult(File.Exists(id));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns></returns>
        public Task AddAsync(FileEntity file, CancellationToken cancellationToken)
        {
            _unitOfWork.Files.Add(file);
            return Task.FromResult(0);
            //#if NETSTANDARD2_1
            //            await File.WriteAllBytesAsync(file.Id, file.Contents, cancellationToken);
            //#else
            //            File.WriteAllBytes(file.Id, file.Contents);
            //            await Task.FromResult(0);
            //#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns></returns>
        public async Task<FileEntity> GetAsync(string id, CancellationToken cancellationToken)
        {
            var contents = await ReadAllBytesAsync(id, cancellationToken);

            return FileEntity.Create(id, Path.GetFileName(id), Path.GetExtension(id),
                MimeMappingEntity.GetMimeMapping(id), contents);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns></returns>
        public Task DeleteAsync(string id, CancellationToken cancellationToken)
        {
            File.Delete(id);
            return Task.FromResult(0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="destFileName"></param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns></returns>
        public Task CopyAsync(FileEntity sourceFile, FileEntity destFileName, CancellationToken cancellationToken)
        {
            File.Copy(sourceFile.Id, destFileName.Id);
            return Task.FromResult(0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="destFile"></param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns></returns>
        public Task MoveAsync(FileEntity sourceFile, FileEntity destFile, CancellationToken cancellationToken)
        {
            File.Move(sourceFile.Id, destFile.Id);
            return Task.FromResult(0);
        }

    }
}
