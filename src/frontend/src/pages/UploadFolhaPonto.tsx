import { useState, useRef } from 'react';
import type { DragEvent, ChangeEvent } from 'react';
import { uploadResumoFolha } from '../api/folhaPontoApi';
import type { OperacaoAceitaOutputDTO } from '../api/folhaPontoApi';

interface Props {
  empresaId: string;
}

export default function UploadFolhaPonto({ empresaId }: Props) {
  const [arquivo, setArquivo] = useState<File | null>(null);
  const [periodoInicio, setPeriodoInicio] = useState('');
  const [periodoFim, setPeriodoFim] = useState('');
  const [dragging, setDragging] = useState(false);
  const [carregando, setCarregando] = useState(false);
  const [resultado, setResultado] = useState<OperacaoAceitaOutputDTO | null>(null);
  const [erro, setErro] = useState<string | null>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  function handleDrop(e: DragEvent<HTMLDivElement>) {
    e.preventDefault();
    setDragging(false);
    const file = e.dataTransfer.files[0];
    if (file) setArquivo(file);
  }

  function handleFileChange(e: ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (file) setArquivo(file);
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!arquivo || !periodoInicio || !periodoFim) return;

    setCarregando(true);
    setErro(null);
    setResultado(null);

    try {
      const resposta = await uploadResumoFolha(empresaId, arquivo, periodoInicio, periodoFim);
      setResultado(resposta);
      setArquivo(null);
    } catch (err: unknown) {
      setErro(err instanceof Error ? err.message : 'Erro desconhecido');
    } finally {
      setCarregando(false);
    }
  }

  return (
    <div className="max-w-xl mx-auto p-6">
      <h1 className="text-2xl font-bold text-gray-800 mb-1">Upload de Folha de Ponto</h1>
      <p className="text-sm text-gray-500 mb-6">
        Envie o arquivo de ponto (CSV, XLSX ou TXT). O processamento ocorre em segundo plano.
      </p>

      <form onSubmit={handleSubmit} className="space-y-5">
        {/* Dropzone */}
        <div
          onDrop={handleDrop}
          onDragOver={(e) => { e.preventDefault(); setDragging(true); }}
          onDragLeave={() => setDragging(false)}
          onClick={() => inputRef.current?.click()}
          className={`border-2 border-dashed rounded-lg p-8 text-center cursor-pointer transition-colors
            ${dragging
              ? 'border-indigo-500 bg-indigo-50'
              : 'border-gray-300 hover:border-indigo-400 hover:bg-gray-50'
            }`}
        >
          <input
            ref={inputRef}
            type="file"
            accept=".csv,.xlsx,.txt"
            className="hidden"
            onChange={handleFileChange}
          />
          {arquivo ? (
            <p className="text-indigo-700 font-medium">✓ {arquivo.name}</p>
          ) : (
            <>
              <p className="text-gray-500 text-sm">Arraste o arquivo aqui ou clique para selecionar</p>
              <p className="text-gray-400 text-xs mt-1">CSV · XLSX · TXT</p>
            </>
          )}
        </div>

        {/* Período */}
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-xs font-medium text-gray-600 mb-1">Início do período</label>
            <input
              type="date"
              value={periodoInicio}
              onChange={(e) => setPeriodoInicio(e.target.value)}
              required
              className="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
          </div>
          <div>
            <label className="block text-xs font-medium text-gray-600 mb-1">Fim do período</label>
            <input
              type="date"
              value={periodoFim}
              onChange={(e) => setPeriodoFim(e.target.value)}
              required
              className="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
          </div>
        </div>

        {/* Botão */}
        <button
          type="submit"
          disabled={!arquivo || !periodoInicio || !periodoFim || carregando}
          className="w-full bg-indigo-600 hover:bg-indigo-700 disabled:bg-gray-300 disabled:cursor-not-allowed
            text-white font-semibold py-2.5 rounded-md text-sm transition-colors"
        >
          {carregando ? 'Enviando...' : 'Enviar para processamento'}
        </button>
      </form>

      {/* Feedback */}
      {resultado && (
        <div className="mt-5 rounded-md bg-green-50 border border-green-200 p-4">
          <p className="text-green-800 font-medium text-sm">✓ {resultado.mensagem}</p>
          <p className="text-green-600 text-xs mt-1">
            ID do processo: <span className="font-mono">{resultado.processoId}</span>
          </p>
        </div>
      )}

      {erro && (
        <div className="mt-5 rounded-md bg-red-50 border border-red-200 p-4">
          <p className="text-red-700 text-sm">✗ {erro}</p>
        </div>
      )}
    </div>
  );
}
