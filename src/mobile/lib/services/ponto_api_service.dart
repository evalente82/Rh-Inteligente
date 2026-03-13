import 'dart:convert';
import 'package:http/http.dart' as http;
import '../models/alerta_anomalia_model.dart';

/// Serviço de acesso à API REST do Vcorp Folha IA.
/// Em produção, a baseUrl será configurada via variável de ambiente / flavors.
class PontoApiService {
  final String baseUrl;
  final http.Client _client;

  PontoApiService({
    required this.baseUrl,
    http.Client? client,
  }) : _client = client ?? http.Client();

  /// Busca anomalias de um funcionário no período informado.
  /// Corresponde a GET /api/folhaponto/{empresaId}/funcionarios/{funcionarioId}/anomalias
  Future<List<AlertaAnomaliaModel>> buscarAnomalias({
    required String empresaId,
    required String funcionarioId,
    required String periodoInicio,
    required String periodoFim,
  }) async {
    final uri = Uri.parse(
      '$baseUrl/api/folhaponto/$empresaId/funcionarios/$funcionarioId/anomalias',
    ).replace(queryParameters: {
      'periodoInicio': periodoInicio,
      'periodoFim': periodoFim,
    });

    final response = await _client.get(uri, headers: {'Accept': 'application/json'});

    if (response.statusCode == 200) {
      final List<dynamic> lista = jsonDecode(response.body) as List<dynamic>;
      return lista
          .map((e) => AlertaAnomaliaModel.fromJson(e as Map<String, dynamic>))
          .toList();
    }

    // Tenta extrair o detail do ProblemDetails (RFC 7807)
    try {
      final problem = jsonDecode(response.body) as Map<String, dynamic>;
      throw Exception(problem['detail'] ?? 'Erro ${response.statusCode}');
    } catch (_) {
      throw Exception('Erro ${response.statusCode}');
    }
  }
}
