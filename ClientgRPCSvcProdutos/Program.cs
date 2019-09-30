using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Net.Client;
using gRPCServiceProdutos;

namespace ClientgRPCSvcProdutos
{
    class Program
    {
        private const string SERVER_GRPC = "https://localhost:5001";
        //private const string SERVER_GRPC = "http://localhost:15001";

        private static async Task IncluirProduto(
            string codigoBarras, string nome, double preco,
            ProdutoSvc.ProdutoSvcClient client)
        {
            Console.WriteLine($"Incluindo o Produto {codigoBarras}");

            var resultado = await client.IncluirAsync(
                new DadosProduto()
                {
                    CodigoBarras = codigoBarras,
                    Nome = nome,
                    Preco = preco
                });

            ImprimirResultado(resultado);
        }

        private static async Task AlterarProduto(
            string codigoBarras, string nome, double preco,
            ProdutoSvc.ProdutoSvcClient client)
        {
            Console.WriteLine($"Alterando o Produto {codigoBarras}");

            var resultado = await client.AlterarAsync(
                new DadosProduto()
                {
                    CodigoBarras = codigoBarras,
                    Nome = nome,
                    Preco = preco
                });

            ImprimirResultado(resultado);
        }

        private static void ImprimirResultado(ProdutoReply resultado)
        {
            Console.WriteLine(resultado.Mensagem);
            if (!resultado.Sucesso)
            {
                Console.WriteLine(
                    $"Inconsistências: {resultado.Inconsistencias}");
            }
            Console.WriteLine();
        }

        private static async Task ListarProdutos(
            ProdutoSvc.ProdutoSvcClient client)
        {
            Console.WriteLine("Produtos cadastrados:");
            using (var call = client.Listar(new ListarProdutosRequest()))
            {
                var responseStream = call.ResponseStream;

                CancellationTokenSource cts = new CancellationTokenSource();
                var token = cts.Token;

                while (await responseStream.MoveNext(token))
                {
                    var dadosProduto = responseStream.Current.Produto;
                    Console.WriteLine(
                        dadosProduto.CodigoBarras + " | " +
                        dadosProduto.Nome + " | " +
                        dadosProduto.Preco);
                }
            }

            Console.WriteLine();
        }

        public static async Task Main()
        {
            if (!SERVER_GRPC.StartsWith("https"))
            {
                AppContext.SetSwitch(
                    "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            }

            var channel = GrpcChannel.ForAddress(SERVER_GRPC);
            var client = new ProdutoSvc.ProdutoSvcClient(channel);

            await IncluirProduto("00001", "Televisão", 2000.57, client);
            await IncluirProduto("00002", "Notebook", 5123.78, client);
            await ListarProdutos(client);

            await AlterarProduto("00002", "Notebook X",
                new Random().Next(5000, 6000), client);
            await ListarProdutos(client);
        }
    }
}