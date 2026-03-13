/// Espelha o AlertaAnomaliaOutputDTO da API C#.
class AlertaAnomaliaModel {
  final String id;
  final String funcionarioId;
  final String tipoAnomalia;
  final String dataReferencia;
  final String descricao;
  final int gravidade;
  final String geradoEm;
  final bool resolvido;
  final String? resolvidoEm;

  const AlertaAnomaliaModel({
    required this.id,
    required this.funcionarioId,
    required this.tipoAnomalia,
    required this.dataReferencia,
    required this.descricao,
    required this.gravidade,
    required this.geradoEm,
    required this.resolvido,
    this.resolvidoEm,
  });

  factory AlertaAnomaliaModel.fromJson(Map<String, dynamic> json) {
    return AlertaAnomaliaModel(
      id: json['id'] as String,
      funcionarioId: json['funcionarioId'] as String,
      tipoAnomalia: json['tipoAnomalia'] as String,
      dataReferencia: json['dataReferencia'] as String,
      descricao: json['descricao'] as String,
      gravidade: json['gravidade'] as int,
      geradoEm: json['geradoEm'] as String,
      resolvido: json['resolvido'] as bool,
      resolvidoEm: json['resolvidoEm'] as String?,
    );
  }
}
