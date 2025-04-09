
```markdown
# Resumo dos Tipos de Métodos LINQ em C#

## Índice
1. [Métodos de Filtragem](#métodos-de-filtragem)
2. [Métodos de Projeção](#métodos-de-projeção)
3. [Métodos de Ordenação](#métodos-de-ordenação)
4. [Métodos de Agregação](#métodos-de-agregação)
5. [Métodos de Junção](#métodos-de-junção)
6. [Métodos de Conjunto](#métodos-de-conjunto)
7. [Métodos de Quantificação](#métodos-de-quantificação)
8. [Métodos de Elementos](#métodos-de-elementos)
9. [Métodos de Geração](#métodos-de-geração)

---

## Métodos de Filtragem
- **Where**: Filtra uma coleção com base em uma condição específica.
- **OfType**: Filtra elementos de um tipo específico em uma coleção.

## Métodos de Projeção
- **Select**: Transforma cada elemento de uma coleção em um novo formato.
- **SelectMany**: Projeta cada elemento em uma sequência e achata o resultado em uma única coleção.

## Métodos de Ordenação
- **OrderBy**: Ordena os elementos de uma coleção em ordem crescente.
- **OrderByDescending**: Ordena os elementos em ordem decrescente.
- **ThenBy**: Realiza uma ordenação secundária em uma coleção já ordenada.
- **Reverse**: Inverte a ordem dos elementos de uma coleção.

## Métodos de Agregação
- **Count**: Retorna o número de elementos em uma coleção.
- **Sum**: Calcula a soma dos valores numéricos de uma coleção.
- **Min**: Retorna o valor mínimo da coleção.
- **Max**: Retorna o valor máximo da coleção.
- **Average**: Calcula a média dos valores numéricos.
- **Aggregate**: Aplica uma função acumuladora personalizada sobre os elementos.

## Métodos de Junção
- **Join**: Combina duas coleções com base em uma chave comum.
- **GroupJoin**: Agrupa elementos de duas coleções com base em uma chave, semelhante a um LEFT JOIN.

## Métodos de Conjunto
- **Distinct**: Remove duplicatas de uma coleção.
- **Union**: Combina duas coleções, eliminando duplicatas.
- **Intersect**: Retorna os elementos comuns entre duas coleções.
- **Except**: Retorna os elementos de uma coleção que não estão em outra.

## Métodos de Quantificação
- **Any**: Verifica se algum elemento da coleção atende a uma condição.
- **All**: Verifica se todos os elementos atendem a uma condição.
- **Contains**: Verifica se um elemento específico está na coleção.

## Métodos de Elementos
- **First**: Retorna o primeiro elemento da coleção (ou que atende a uma condição).
- **FirstOrDefault**: Retorna o primeiro elemento ou um valor padrão se não houver.
- **Last**: Retorna o último elemento da coleção.
- **LastOrDefault**: Retorna o último elemento ou um valor padrão.
- **Single**: Retorna o único elemento que atende a uma condição (lança exceção se houver mais de um).
- **SingleOrDefault**: Retorna o único elemento ou um valor padrão.
- **ElementAt**: Retorna o elemento em um índice específico.

## Métodos de Geração
- **Range**: Gera uma sequência de números inteiros.
- **Repeat**: Gera uma coleção repetindo um valor específico.
- **Empty**: Retorna uma coleção vazia do tipo especificado.
```

### Explicação
Este resumo cobre os principais métodos do LINQ (Language Integrated Query) em C#, que são amplamente utilizados para manipulação de coleções (como listas, arrays, etc.). Eles podem ser usados tanto na sintaxe de método quanto na sintaxe de consulta (query). Cada método é otimizado para tarefas específicas, como filtrar, transformar, ordenar ou agregar dados, tornando o LINQ uma ferramenta poderosa e expressiva no desenvolvimento em C#.