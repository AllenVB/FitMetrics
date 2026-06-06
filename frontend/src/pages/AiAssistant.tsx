import { useEffect, useRef, useState, type FormEvent } from 'react';
import { aiApi } from '../api';
import { tokenStorage } from '../api/client';
import { Button, Card, ErrorAlert, Input, PageHeader, Spinner } from '../components/ui';
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
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${tokenStorage.get() ?? ''}`,
        },
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
      <PageHeader title="💬 AI Asistan" subtitle="Verilerine dayalı kişisel sağlık & fitness sohbeti" />

      {!enabled ? (
        <Card className="border-l-4 border-amber-400">
          <h3 className="mb-1 font-semibold text-slate-800">⚠️ AI Asistan şu an kapalı</h3>
          <p className="text-sm text-slate-600">
            Sohbet için sunucuda <code className="rounded bg-slate-100 px-1">ANTHROPIC_API_KEY</code> tanımlı olmalı.
            Uygulamanın geri kalanı anahtar olmadan da çalışır.
          </p>
        </Card>
      ) : (
        <Card>
          <div className="space-y-3 overflow-y-auto pr-1" style={{ maxHeight: '58vh', minHeight: '40vh' }}>
            {messages.length === 0 && (
              <div className="py-8 text-center">
                <p className="mb-4 text-sm text-slate-500">Merhaba! Verilerine bakarak sorularını yanıtlarım. Bir şeyler dene:</p>
                <div className="flex flex-wrap justify-center gap-2">
                  {SUGGESTIONS.map((s) => (
                    <button
                      key={s}
                      onClick={() => send(s)}
                      className="rounded-full border border-slate-200 bg-white px-3 py-1.5 text-sm text-slate-600 transition hover:border-brand-400 hover:text-brand-700"
                    >
                      {s}
                    </button>
                  ))}
                </div>
              </div>
            )}

            {messages.map((m, i) => (
              <div key={i} className={`flex ${m.role === 'user' ? 'justify-end' : 'justify-start'}`}>
                <div
                  className={`max-w-[80%] whitespace-pre-line rounded-2xl px-4 py-2 text-sm leading-relaxed ${
                    m.role === 'user' ? 'bg-brand-600 text-white' : 'bg-slate-100 text-slate-700'
                  }`}
                >
                  {m.content || '…'}
                </div>
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
