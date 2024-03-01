using Fila_Unica_BQ.Models;
using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fila_Unica_BQ.Services
{
    public class FirebaseService
    {
        readonly FirebaseClient firebase = new FirebaseClient("https://fila-unica-brusque-default-rtdb.firebaseio.com/");

        readonly string codURL = OrigemInscricao.Origem;

        public async Task AddCandidato(int posicao, int protocoloid, string data_dora, string opcao1, string opcao2, string opcao3, string chamadas)
        {
            await firebase.Child("Candidatos" + codURL).PostAsync(new Candidato()
            {
                Posicao = posicao,
                ProtocoloId = protocoloid,
                Data_Hora = data_dora,
                Opcao1 = opcao1,
                Opcao2 = opcao2,
                Opcao3 = opcao3,
                Chamadas = chamadas
            });
        }

        public async Task<List<Candidato>> GetCandidatos()
        {
            return (await firebase.Child("Candidatos" + codURL).OnceAsync<Candidato>()).Select(item => new Candidato
            {
                Posicao = item.Object.Posicao,
                ProtocoloId = item.Object.ProtocoloId,
                Data_Hora = item.Object.Data_Hora,
                Opcao1 = item.Object.Opcao1,
                Opcao2 = item.Object.Opcao2,
                Opcao3 = item.Object.Opcao3,
                Chamadas = item.Object.Chamadas
            }).ToList();
        }

        public async Task<Candidato> GetCandidato(int ProtocoloId)
        {
            var candidatos = await GetCandidatos();
            await firebase.Child("Candidatos" + codURL).OnceAsync<Candidato>();
            return candidatos.Where(a => a.ProtocoloId == ProtocoloId).FirstOrDefault();
        }

        public async Task AddEscolaCandidato(int CodEscola, string NomeEscola, int opcao1, int opcao2, int opcao3)
        {
            await firebase.Child("EscolaCandidatos").PostAsync(new EscolaCandidato()
            {
                EscolaCod = CodEscola,
                EscolaNome = NomeEscola,
                Opcao1 = opcao1,
                Opcao2 = opcao2,
                Opcao3 = opcao3,
                Opcoes = "1ª Opção: " + opcao1 + "  |  2ª Opção: " + opcao2 + "  |  3ª Opção: " + opcao3
            });
        }

        public async Task<List<EscolaCandidato>> GetEscolaCandidatos()
        {
            return (await firebase.Child("EscolaCandidatos").OnceAsync<EscolaCandidato>()).Select(item => new EscolaCandidato
            {
                EscolaCod = item.Object.EscolaCod,
                EscolaNome = item.Object.EscolaNome,
                Opcao1 = item.Object.Opcao1,
                Opcao2 = item.Object.Opcao2,
                Opcao3 = item.Object.Opcao3,
                Opcoes = item.Object.Opcoes
            }).ToList();
        }

        public async Task<EscolaCandidato> GetEscolaCandidato(int codigo)
        {
            var escolas = await GetEscolaCandidatos();
            await firebase.Child("EscolaCandidatos").OnceAsync<EscolaCandidato>();
            return escolas.Where(a => a.EscolaCod == codigo).FirstOrDefault();
        }

        public async Task AddInfoEscola(int codigo,
                                        string endereco, 
                                        string bairro,
                                        string cep,
                                        string cnpj,
                                        string fone,
                                        string email,
                                        string diretor,
                                        string coordenador,
                                        string presidente)
        {
            await firebase.Child("InfoEscola").PostAsync(new InfoEscola()
            {
                EscolaCod = codigo,
                EscolaEnd = endereco,
                EscolaBairro = bairro,
                EscolaCEP = cep,
                EscolaCNPJ = cnpj,
                EscolaFone = fone,
                EscolaEmail = email,
                EscolaDiretor = diretor,
                EscolaCoord = coordenador,
                EscolaPresidente = presidente
            });
        }

        public async Task UpdateEscola(int codigo,
                                        string endereco,
                                        string bairro,
                                        string cep,
                                        string cnpj,
                                        string fone,
                                        string email,
                                        string diretor,
                                        string coordenador,
                                        string presidente)
        {
            var toUpdateContato = (await firebase
              .Child("InfoEscola")
                .OnceAsync<InfoEscola>())
                   .Where(a => a.Object.EscolaCod == codigo).FirstOrDefault();
            await firebase
              .Child("InfoEscola")
                .Child(toUpdateContato.Key)
                  .PutAsync(new InfoEscola()
                  {
                      EscolaCod = codigo,
                      EscolaEnd = endereco,
                      EscolaBairro = bairro,
                      EscolaCEP = cep,
                      EscolaCNPJ = cnpj,
                      EscolaFone = fone,
                      EscolaEmail = email,
                      EscolaDiretor = diretor,
                      EscolaCoord = coordenador,
                      EscolaPresidente = presidente
                  });
        }

        public async Task<List<InfoEscola>> GetEscolas()
        {
            string Nome_Escola = "";
            var escola = await GetEscolaCandidato(Dados_gerais.CodigoEscola);
            if (escola != null)
            {
                Nome_Escola = escola.EscolaNome;
            }

            return (await firebase.Child("InfoEscola").OnceAsync<InfoEscola>()).Select(item => new InfoEscola
            {
                EscolaCod = item.Object.EscolaCod,
                EscolaEnd = item.Object.EscolaEnd,
                EscolaBairro = item.Object.EscolaBairro,
                EscolaCEP = item.Object.EscolaCEP,
                EscolaCNPJ = item.Object.EscolaCNPJ,
                EscolaFone = item.Object.EscolaFone,
                EscolaEmail=item.Object.EscolaEmail,
                EscolaNome = Nome_Escola,
                EscolaDiretor = item.Object.EscolaDiretor,
                EscolaCoord = item.Object.EscolaCoord,
                EscolaPresidente = item.Object.EscolaPresidente
            }).ToList();
        }

        public async Task<InfoEscola> GetEscola(int codescola)
        {
            var escolas = await GetEscolas();
            await firebase.Child("InfoEscola").OnceAsync<InfoEscola>();
            return escolas.Where(a => a.EscolaCod == codescola).FirstOrDefault();
        }


        public async Task Atualiza_Data()
        {
            await firebase.Child("DataAtualizacao" + codURL).DeleteAsync();
            await firebase.Child("DataAtualizacao" + codURL).PostAsync(new Atualizacao() { Ultima_atualizacao = DateTime.Now.Date });
        }

        public async Task DeletaTudo()
        {
            await firebase.Child("Candidatos" + codURL).DeleteAsync();
        }

        public async Task DeletaEscolas()
        {
            await firebase.Child("EscolaCandidatos").DeleteAsync();
        }
    }
}
