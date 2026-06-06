import { useEffect, useState } from 'react';
import { knowledgeApi } from '../api';
import { getErrorMessage } from '../api/client';
import type { KnowledgeEntry } from '../types';
import {
  Button, Card, EmptyState, ErrorAlert, Field, Icon, Input, PageHeader, Spinner, Textarea,
} from '../components/ui';

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

      <Card className="border-l-4 border-l-primary">
        <div className="flex gap-3 text-body-sm text-on-surface-variant">
          <Icon name="lightbulb" className="text-xl text-primary" />
          <p>
            Buraya eklediğiniz her <strong className="text-on-surface">soru &amp; cevap</strong>, AI Asistan'a{' '}
            <strong className="text-on-surface">onaylı bilgi</strong> olarak verilir. Kullanıcı benzer bir şey
            sorduğunda asistan, kendi genel bilgisi yerine sizin yazdığınız cevabı esas alıp doğal bir dille aktarır.
          </p>
        </div>
      </Card>

      <Card>
        <h2 className="mb-4 text-title-md font-bold text-on-surface">Yeni Bilgi Ekle</h2>
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
            <Textarea
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
        <h2 className="mb-3 text-title-md font-bold text-on-surface">
          Kayıtlı Bilgiler {entries.length > 0 && <span className="text-on-surface-variant">({entries.length})</span>}
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
                    <div className="flex items-center gap-2 text-body-sm font-semibold text-on-surface">
                      <Icon name="help" className="text-primary" />
                      <span className="break-words">{entry.question}</span>
                    </div>
                    <p className="mt-2 whitespace-pre-wrap break-words text-body-sm text-on-surface-variant">{entry.answer}</p>
                  </div>
                  <button
                    onClick={() => handleDelete(entry.id)}
                    className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg text-on-surface-variant transition-colors hover:bg-error/10 hover:text-error"
                    title="Sil"
                  >
                    <Icon name="delete" className="text-lg" />
                  </button>
                </div>
              </Card>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
