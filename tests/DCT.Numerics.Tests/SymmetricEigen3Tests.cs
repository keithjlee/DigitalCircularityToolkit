using System;
using DigitalCircularityToolkit.Numerics;
using Xunit;

public class SymmetricEigen3Tests
{
    static double[,] Reconstruct(double[] values, double[][] vectors)
    {
        var a = new double[3, 3];
        for (int k = 0; k < 3; k++)
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    a[i, j] += values[k] * vectors[k][i] * vectors[k][j];
        return a;
    }

    [Fact]
    public void Diagonal_matrix_gives_sorted_axis_vectors()
    {
        SymmetricEigen3.Decompose(new double[,] { { 1, 0, 0 }, { 0, 5, 0 }, { 0, 0, 3 } }, out var values, out var vectors);
        Assert.Equal(new[] { 5.0, 3.0, 1.0 }, values);
        Assert.Equal(1.0, Math.Abs(vectors[0][1]), 12);
        Assert.Equal(1.0, Math.Abs(vectors[1][2]), 12);
        Assert.Equal(1.0, Math.Abs(vectors[2][0]), 12);
    }

    [Fact]
    public void Random_symmetric_matrices_are_reconstructed_by_orthonormal_descending_eigenpairs()
    {
        var rng = new Random(42);
        for (int trial = 0; trial < 500; trial++)
        {
            var m = new double[3, 3];
            for (int i = 0; i < 3; i++)
                for (int j = i; j < 3; j++)
                    m[i, j] = m[j, i] = rng.NextDouble() * 200 - 100;

            SymmetricEigen3.Decompose(m, out var values, out var vectors);

            Assert.True(values[0] >= values[1] && values[1] >= values[2]);
            var r = Reconstruct(values, vectors);
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    Assert.Equal(m[i, j], r[i, j], 9);
            for (int a = 0; a < 3; a++)
                for (int b = 0; b < 3; b++)
                {
                    double dot = vectors[a][0] * vectors[b][0] + vectors[a][1] * vectors[b][1] + vectors[a][2] * vectors[b][2];
                    Assert.Equal(a == b ? 1.0 : 0.0, dot, 12);
                }
        }
    }

    [Fact]
    public void Input_matrix_is_not_modified_and_zero_matrix_is_handled()
    {
        var zero = new double[3, 3];
        SymmetricEigen3.Decompose(zero, out var values, out var vectors);
        Assert.Equal(new[] { 0.0, 0.0, 0.0 }, values);
        Assert.Equal(new double[3, 3], zero);
        Assert.Equal(1.0, vectors[0][0] * vectors[0][0] + vectors[0][1] * vectors[0][1] + vectors[0][2] * vectors[0][2], 12);
    }
}
