using AutoMapper;
using EBL.FIG.Process.Identity.Application.AutoMapper;

namespace EBL.FIG.Process.Identity.Test;

public class TestAutoMapperSetup
{
    public static void TestSetup()
    {
        // Teste para descobrir a API correta do AutoMapper 16.1.1
        var profiles = new[] 
        {
            new UserMappingProfile()
        };
        
        // Tenta várias abordagens
    }
}
