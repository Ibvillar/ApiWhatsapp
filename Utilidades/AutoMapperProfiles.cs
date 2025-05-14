using ApiWhatsapp.DTO;
using ApiWhatsapp.Entitties;
using AutoMapper;

namespace ApiWhatsapp.Utilidades
{
    public class AutoMapperProfiles: Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<TelefonoDTO, Telefono>()
                .ForMember(dto => dto.Id, config => config.MapFrom(
                    ent => long.Parse(ent.Prefijo.ToString() + ent.Numero.ToString())));
        }
    }
}
