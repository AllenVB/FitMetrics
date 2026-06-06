import { useEffect, useState } from 'react';
import { knowledgeApi } from '../api';
import { getErrorMessage } from '../api/client';
import type { KnowledgeEntry } from '../types';
import {
  Button, Card, EmptyState, ErrorAlert, Field, Input, PageHeader, Spinner,
} from '../components/ui';

const textareaClasses =
  'w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-800 outline-none transition focus:border-brand-500 focus:ring-2 focus:ring-brand-100';

export default function Knowledge() {
  const [entries, setEntries] = useState<KnowledgeEntry[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [question, setQuestion] = useState('');
  const [answer, setAnswer] = useState('');
  const [saving, setSaving] = useState(false);

  const load = async () => {
    try {
      setLoading(true);
      setEntries(await knowledgeApi.getAll());
      setError('');
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!question.trim() || !answer.trim()) return;
    try {
      setSaving(true);
      await knowledgeApi.create({ question: question.trim(), answer: answer.trim() });
      setQuestion('');
      setAnswer('');
      setError('');
      await load();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id: number) => {
    try {
      await knowledgeApi.remove(id);
      setEntries((prev) => prev.filter((x) => x.id !== id));
    } catch (err) {
      setError(getErrorMessage(err));
    }
  };

  return (
    <div className="mx-auto max-w-4xl space-y-6">
      <PageHeader
        title="Bilgi Tabanı"
        subtitle="AI Asistan'ın vereceği cevapları burada belirleyin. Eklediğiniz soru-cevaplar, asistanın yanıtlarına temel alınır."
      />

      <Card className="border-brand-100 bg-brand-50/50">
        <div className="flex gap-3 text-sm text-slate-600">
          <span className="text-xl">💡</span>
          <p>
            Buraya eklediğiniz her <strong>soru &amp; cevap</strong>, AI Asistan'a <strong>onaylı bilgi</strong> olarak
            verilir. Kullanıcı benzer bir şey sorduğunda asistan, kendi genel bilgisi yerine sizin yazdığınız cevabı
            esas alıp doğal bir dille aktarır.
          </p>
        </div>
      </Card>

      <Card>
        <h2 className="mb-4 text-lg font-semibold text-slate-800">Yeni Bilgi Ekle</h2>
        <form onSubmit={handleSubmit} className="space-y-4">
          <Field label="Soru / Konu" hint="Kullanıcının sorabileceği soru ya da anahtar konu.">
            <Input
              value={question}
              onChange={(e) => setQuestion(e.target.value)}
              placeholder="Örn: Protein hedefimi nasıl artırırım?"
              maxLength={500}
            />
          </Field>
          <Field label="Onaylı Cevap" hint="Asistanın bu konuda vereceği cevabın içeriği.">
            <textarea
              className={textareaClasses}
              value={answer}
              onChange={(e) => setAnswer(e.target.value)}
              placeholder="Örn: Günlük protein hedefini kademeli artır; her öğüne bir protein kaynağı ekle…"
              rows={4}
              maxLength={4000}
            />
          </Field>
          <ErrorAlert message={error} />
          <div className="flex justify-end">
            <Button type="submit" disabled={saving || !question.trim() || !answer.trim()}>
              {saving ? 'Kaydediliyor…' : 'Ekle'}
            </Button>
          </div>
        </form>
      </Card>

      <div>
        <h2 className="mb-3 text-lg font-semibold text-slate-800">
          Kayıtlı Bilgiler {entries.length > 0 && <span className="text-slate-400">({entries.length})</span>}
        </h2>
        {loading ? (
          <Spinner />
        ) : entries.length === 0 ? (
          <EmptyState message="Henüz bilgi eklenmedi. Yukarıdan ilk soru-cevabınızı ekleyin." />
        ) : (
          <div className="space-y-3">
            {entries.map((entry) => (
              <Card key={entry.id}>
                <div className="flex items-start justify-between gap-4">
                  <div className="min-w-0 flex-1">
                    <div className="flex items-center gap-2 text-sm font-semibold text-slate-800">
                      <span className="text-brand-500">❓</span>
                      <span className="break-words">{entry.question}</span>
                    </div>
                    <p className="mt-2 whitespace-pre-wrap break-words text-sm text-slate-600">{entry.answer}</p>
                  </div>
                  <Button variant="ghost" onClick={() => handleDelete(entry.id)} title="Sil">
                    🗑️
                  </Button>
                </div>
              </Card>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
