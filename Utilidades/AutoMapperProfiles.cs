using ApiWhatsapp.DTO;
using ApiWhatsapp.Entitties;

namespace ApiWhatsapp.Utilidades
{
    public class AutoMapperProfiles: AutoMapper.Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<TelefonoDTO, Telefono>()
                .ForMember(dto => dto.Id, config => config.MapFrom(
                    ent => long.Parse(ent.Prefijo.ToString() + ent.Numero.ToString())));

            CreateMap<TelefonoWithGenerales, Telefono>()
                .ForMember(dto => dto.Id, config => config.MapFrom(
                    ent => long.Parse(ent.Prefijo.ToString() + ent.Numero.ToString())));

            CreateMap<FicheroDTO, FicheroConExtensionDTO>();
            CreateMap<FicheroConExtensionDTO, Fichero>();
        }
    }
}
