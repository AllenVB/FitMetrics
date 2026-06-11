import { useEffect, useRef, useState, type FormEvent } from 'react';
import { aiApi } from '../api';
import { Button, Card, ErrorAlert, Icon, Input, PageHeader, Spinner } from '../components/ui';
import type { ChatMessage } from '../types';

const SUGGESTIONS = [
  'Bugün protein hedefimden ne kadar uzaktayım?',
  'Kilo vermem için bugün ne önerirsin?',
  'Bu haftaki antrenmanım dengeli mi?',
  'Akşam için 600 kcal sağlıklı bir öneri ver.',
];

export default function AiAssistant() {
  const [enabled, setEnabled] = useState<boolean | null>(null);
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [input, setInput] = useState('');
  const [sending, setSending] = useState(false);
  const [error, setError] = useState('');
  const endRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    aiApi.status().then((s) => setEnabled(s.enabled)).catch(() => setEnabled(false));
  }, []);

  useEffect(() => {
    endRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages, sending]);

  // Kalıcı sohbet geçmişini yükle (AI etkinse)
  useEffect(() => {
    if (enabled) aiApi.chatHistory().then(setMessages).catch(() => { /* sessiz */ });
  }, [enabled]);

  const clearHistory = async () => {
    try {
      await aiApi.clearChat();
      setMessages([]);
      setError('');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Geçmiş temizlenemedi.');
    }
  };

  const send = async (text: string) => {
    const content = text.trim();
    if (!content || sending) return;
    setError('');
    const base: ChatMessage[] = [...messages, { role: 'user', content }];
    setMessages([...base, { role: 'assistant', content: '' }]); // boş baloncuk → akışla dolar
    setInput('');
    setSending(true);
    try {
      const res = await fetch('/api/ai/chat/stream', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ messages: base }),
      });
      if (!res.ok || !res.body) {
        let msg = 'Yanıt alınamadı.';
        try {
          const j = await res.json();
          msg = j.message ?? msg;
        } catch { /* gövde metin değil */ }
        throw new Error(msg);
      }
      const reader = res.body.getReader();
      const decoder = new TextDecoder();
      let acc = '';
      for (;;) {
        const { done, value } = await reader.read();
        if (done) break;
        acc += decoder.decode(value, { stream: true });
        setMessages([...base, { role: 'assistant', content: acc }]);
      }
      if (!acc.trim()) setMessages([...base, { role: 'assistant', content: '(boş yanıt)' }]);
    } catch (err) {
      setMessages(base); // boş baloncuğu kaldır
      setError(err instanceof Error ? err.message : 'Bir hata oluştu.');
    } finally {
      setSending(false);
    }
  };

  const onSubmit = (e: FormEvent) => {
    e.preventDefault();
    send(input);
  };

  if (enabled === null) return <Spinner />;

  return (
    <div>
      <PageHeader title="AI Asistan" subtitle="Verilerine dayalı kişisel sağlık & fitness sohbeti" />

      {!enabled ? (
        <Card className="border-l-4 border-l-amber-400">
          <h3 className="mb-1 flex items-center gap-2 font-semibold text-on-surface">
            <Icon name="warning" className="text-amber-400" /> AI Asistan şu an kapalı
          </h3>
          <p className="text-body-sm text-on-surface-variant">
            Sohbet için sunucuda bir AI sağlayıcı gerekir. Ollama çalışıyorsa veya{' '}
            <code className="rounded bg-white/10 px-1">Anthropic:ApiKey</code> tanımlıysa otomatik etkinleşir.
            Uygulamanın geri kalanı AI olmadan da çalışır.
          </p>
        </Card>
      ) : (
        <Card>
          {messages.length > 0 && (
            <div className="mb-3 flex items-center justify-between">
              <span className="flex items-center gap-1.5 text-xs text-on-surface-variant">
                <Icon name="history" className="text-sm" /> Sohbet geçmişin kayıtlı
              </span>
              <button
                onClick={clearHistory}
                className="flex items-center gap-1 rounded-lg px-2 py-1 text-xs font-medium text-on-surface-variant transition-colors hover:bg-error/10 hover:text-error"
              >
                <Icon name="delete_sweep" className="text-sm" /> Geçmişi temizle
              </button>
            </div>
          )}
          <div className="custom-scrollbar space-y-5 overflow-y-auto pr-1" style={{ maxHeight: '58vh', minHeight: '40vh' }}>
            {messages.length === 0 && (
              <div className="py-8 text-center">
                <p className="mb-4 text-body-sm text-on-surface-variant">Merhaba! Verilerine bakarak sorularını yanıtlarım. Bir şeyler dene:</p>
                <div className="flex flex-wrap justify-center gap-2">
                  {SUGGESTIONS.map((s) => (
                    <button
                      key={s}
                      onClick={() => send(s)}
                      className="rounded-full border border-white/10 bg-surface-container-high px-3 py-1.5 text-body-sm text-on-surface-variant transition-colors hover:border-primary/40 hover:text-primary"
                    >
                      {s}
                    </button>
                  ))}
                </div>
              </div>
            )}

            {messages.map((m, i) => (
              <div key={i} className={`flex items-start gap-3 ${m.role === 'user' ? 'justify-end' : 'justify-start'}`}>
                {m.role === 'assistant' && (
                  <div className="flex h-7 w-7 shrink-0 items-center justify-center rounded-lg bg-primary-container">
                    <Icon name="psychology" className="text-sm text-on-primary-container" />
                  </div>
                )}
                <div
                  className={`max-w-[80%] whitespace-pre-line rounded-2xl px-4 py-2.5 text-body-sm leading-relaxed ${
                    m.role === 'user'
                      ? 'rounded-tr-none bg-primary text-on-primary'
                      : 'rounded-tl-none border border-white/5 bg-surface-container-high text-on-surface'
                  }`}
                >
                  {m.content || '…'}
                </div>
                {m.role === 'user' && (
                  <div className="flex h-7 w-7 shrink-0 items-center justify-center rounded-lg bg-surface-container">
                    <Icon name="person" className="text-sm text-on-surface-variant" />
                  </div>
                )}
              </div>
            ))}
            <div ref={endRef} />
          </div>

          <div className="mt-3"><ErrorAlert message={error} /></div>

          <form onSubmit={onSubmit} className="mt-3 flex gap-2">
            <Input value={input} onChange={(e) => setInput(e.target.value)} placeholder="Bir şey sor…" />
            <Button type="submit" disabled={sending || !input.trim()}>Gönder</Button>
          </form>
        </Card>
      )}
    </div>
  );
}
