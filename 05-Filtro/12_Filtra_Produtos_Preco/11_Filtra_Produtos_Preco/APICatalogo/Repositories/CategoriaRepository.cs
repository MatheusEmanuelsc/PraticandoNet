﻿using APICatalogo.Context;
using APICatalogo.Models;
using APICatalogo.Pagination;

namespace APICatalogo.Repositories;

public class CategoriaRepository : Repository<Categoria>, ICategoriaRepository
{
    public CategoriaRepository(AppDbContext context) : base(context)
    { }

    public PagedList<Categoria> GetCategorias(CategoriasParameters categoriasParams)
    {
        var categorias = GetAll().OrderBy(p => p.CategoriaId).AsQueryable();

        var categoriasOrdenadas = PagedList<Categoria>.ToPagedList(categorias,
            categoriasParams.PageNumber, categoriasParams.PageSize);

        return categoriasOrdenadas;
    }
}
