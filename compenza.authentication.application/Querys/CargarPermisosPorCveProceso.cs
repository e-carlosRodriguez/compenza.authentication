using compenza.authentication.application.Exceptions;
using compenza.authentication.domain.Configure;
using compenza.authentication.domain.Enums;
using compenza.authentication.percistance.Interfaces;
using MediatR;

namespace compenza.authentication.application.Querys
{
    public class CargarPermisosPorCveProceso
    {
        public record Query( int CveProceso, int cvePerfil, int cveUsuario ): IRequest<Result>;

        public class Handler : IRequestHandler<Query, Result>
        {
            private readonly IProcesosRepository _permisoRepository;

            public Handler(IProcesosRepository permisoRepository)
            {
                _permisoRepository = permisoRepository;
            }

            public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
            {
                var result = new Result();
                var procesoResult = await _permisoRepository.CargarProceso( request.CveProceso, request.cvePerfil, request.cveUsuario );
                if (procesoResult is null)
                    throw new HttpException( System.Net.HttpStatusCode.NotFound, "Process not found", null );

                await _permisoRepository.Auditoria( request.CveProceso, (int)eAcciones.AbrirPantalla, request.cveUsuario );
                result.Objeto = procesoResult;

                return result;
            }
        }
    }
}
