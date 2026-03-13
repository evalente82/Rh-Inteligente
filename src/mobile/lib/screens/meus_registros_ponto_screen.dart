import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../models/alerta_anomalia_model.dart';
import '../services/ponto_api_service.dart';

/// Tela principal: exibe anomalias de ponto auditadas pela IA para um funcionário.
class MeusRegistrosPontoScreen extends StatefulWidget {
  final PontoApiService apiService;
  final String empresaId;
  final String funcionarioId;

  const MeusRegistrosPontoScreen({
    super.key,
    required this.apiService,
    required this.empresaId,
    required this.funcionarioId,
  });

  @override
  State<MeusRegistrosPontoScreen> createState() =>
      _MeusRegistrosPontoScreenState();
}

class _MeusRegistrosPontoScreenState
    extends State<MeusRegistrosPontoScreen> {
  // Período padrão: mês corrente
  DateTime _periodoInicio = DateTime(
    DateTime.now().year,
    DateTime.now().month,
    1,
  );
  DateTime _periodoFim = DateTime.now();

  late Future<List<AlertaAnomaliaModel>> _futureAnomalias;

  final DateFormat _fmt = DateFormat('yyyy-MM-dd');
  final DateFormat _fmtExibir = DateFormat('dd/MM/yyyy', 'pt_BR');

  @override
  void initState() {
    super.initState();
    _buscar();
  }

  void _buscar() {
    setState(() {
      _futureAnomalias = widget.apiService.buscarAnomalias(
        empresaId: widget.empresaId,
        funcionarioId: widget.funcionarioId,
        periodoInicio: _fmt.format(_periodoInicio),
        periodoFim: _fmt.format(_periodoFim),
      );
    });
  }

  Future<void> _selecionarData(BuildContext context, bool isInicio) async {
    final picked = await showDatePicker(
      context: context,
      initialDate: isInicio ? _periodoInicio : _periodoFim,
      firstDate: DateTime(2020),
      lastDate: DateTime(2030),
      locale: const Locale('pt', 'BR'),
    );
    if (picked != null) {
      setState(() {
        if (isInicio) {
          _periodoInicio = picked;
        } else {
          _periodoFim = picked;
        }
      });
      _buscar();
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Meus Registros de Ponto'),
        centerTitle: true,
      ),
      body: Column(
        children: [
          _buildFiltros(context),
          const Divider(height: 1),
          Expanded(child: _buildCorpo()),
        ],
      ),
    );
  }

  /// Barra de filtro de período
  Widget _buildFiltros(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      child: Row(
        children: [
          Expanded(
            child: _CampoPeriodo(
              label: 'De',
              valor: _fmtExibir.format(_periodoInicio),
              onTap: () => _selecionarData(context, true),
            ),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: _CampoPeriodo(
              label: 'Até',
              valor: _fmtExibir.format(_periodoFim),
              onTap: () => _selecionarData(context, false),
            ),
          ),
        ],
      ),
    );
  }

  /// Corpo principal: estado do Future
  Widget _buildCorpo() {
    return FutureBuilder<List<AlertaAnomaliaModel>>(
      future: _futureAnomalias,
      builder: (context, snapshot) {
        if (snapshot.connectionState == ConnectionState.waiting) {
          return const Center(child: CircularProgressIndicator());
        }
        if (snapshot.hasError) {
          return _buildErro(snapshot.error.toString());
        }
        final anomalias = snapshot.data ?? [];
        if (anomalias.isEmpty) {
          return _buildVazio();
        }
        return _buildLista(anomalias);
      },
    );
  }

  Widget _buildErro(String mensagem) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(Icons.error_outline, size: 48, color: Colors.red),
            const SizedBox(height: 12),
            Text(
              'Falha ao carregar dados',
              style: Theme.of(context).textTheme.titleMedium,
            ),
            const SizedBox(height: 4),
            Text(
              mensagem,
              textAlign: TextAlign.center,
              style: const TextStyle(color: Colors.grey),
            ),
            const SizedBox(height: 16),
            FilledButton.icon(
              onPressed: _buscar,
              icon: const Icon(Icons.refresh),
              label: const Text('Tentar novamente'),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildVazio() {
    return Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          const Icon(Icons.check_circle_outline, size: 56, color: Colors.green),
          const SizedBox(height: 12),
          Text(
            'Nenhuma anomalia encontrada',
            style: Theme.of(context).textTheme.titleMedium,
          ),
          const SizedBox(height: 4),
          const Text(
            'Sem alertas para o período selecionado.',
            style: TextStyle(color: Colors.grey),
          ),
        ],
      ),
    );
  }

  Widget _buildLista(List<AlertaAnomaliaModel> anomalias) {
    // Ordena: gravidade decrescente
    final ordenadas = List<AlertaAnomaliaModel>.from(anomalias)
      ..sort((a, b) => b.gravidade.compareTo(a.gravidade));

    return ListView.separated(
      padding: const EdgeInsets.all(12),
      itemCount: ordenadas.length,
      separatorBuilder: (_, __) => const SizedBox(height: 8),
      itemBuilder: (context, index) {
        return _CardAnomalia(anomalia: ordenadas[index]);
      },
    );
  }
}

// ---------------------------------------------------------------------------
// Widgets auxiliares
// ---------------------------------------------------------------------------

class _CampoPeriodo extends StatelessWidget {
  final String label;
  final String valor;
  final VoidCallback onTap;

  const _CampoPeriodo({
    required this.label,
    required this.valor,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(8),
      child: InputDecorator(
        decoration: InputDecoration(
          labelText: label,
          border: const OutlineInputBorder(),
          contentPadding:
              const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
          suffixIcon: const Icon(Icons.calendar_today, size: 18),
        ),
        child: Text(valor),
      ),
    );
  }
}

class _CardAnomalia extends StatelessWidget {
  final AlertaAnomaliaModel anomalia;

  const _CardAnomalia({required this.anomalia});

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;

    return Card(
      elevation: 2,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(10),
        side: BorderSide(color: _corGravidade(anomalia.gravidade), width: 1.5),
      ),
      child: Padding(
        padding: const EdgeInsets.all(14),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Expanded(
                  child: Text(
                    anomalia.tipoAnomalia,
                    style: Theme.of(context)
                        .textTheme
                        .titleSmall
                        ?.copyWith(fontWeight: FontWeight.bold),
                  ),
                ),
                _ChipGravidade(gravidade: anomalia.gravidade),
                const SizedBox(width: 6),
                _ChipStatus(resolvido: anomalia.resolvido),
              ],
            ),
            const SizedBox(height: 8),
            Text(anomalia.descricao),
            const SizedBox(height: 6),
            Text(
              '📅 ${anomalia.dataReferencia}',
              style: TextStyle(
                fontSize: 12,
                color: colorScheme.onSurfaceVariant,
              ),
            ),
          ],
        ),
      ),
    );
  }

  Color _corGravidade(int gravidade) {
    switch (gravidade) {
      case 1:
        return Colors.blue;
      case 2:
        return Colors.orange;
      case 3:
        return Colors.red;
      default:
        return Colors.grey;
    }
  }
}

class _ChipGravidade extends StatelessWidget {
  final int gravidade;

  const _ChipGravidade({required this.gravidade});

  @override
  Widget build(BuildContext context) {
    final (label, cor) = switch (gravidade) {
      1 => ('Informativo', Colors.blue),
      2 => ('Atenção', Colors.orange),
      3 => ('Crítico', Colors.red),
      _ => ('Desconhecido', Colors.grey),
    };

    return Chip(
      label: Text(label,
          style: const TextStyle(fontSize: 11, color: Colors.white)),
      backgroundColor: cor,
      padding: EdgeInsets.zero,
      materialTapTargetSize: MaterialTapTargetSize.shrinkWrap,
    );
  }
}

class _ChipStatus extends StatelessWidget {
  final bool resolvido;

  const _ChipStatus({required this.resolvido});

  @override
  Widget build(BuildContext context) {
    final label = resolvido ? 'Resolvido' : 'Pendente';
    return Chip(
      label: Text(
        label,
        style: TextStyle(
          fontSize: 11,
          color: resolvido ? Colors.green.shade900 : Colors.orange.shade900,
        ),
      ),
      backgroundColor:
          resolvido ? Colors.green.shade100 : Colors.orange.shade100,
      padding: EdgeInsets.zero,
      materialTapTargetSize: MaterialTapTargetSize.shrinkWrap,
    );
  }
}
