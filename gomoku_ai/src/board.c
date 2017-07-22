/* ************************************************************************** */
/*                                                                            */
/*                                                        :::      ::::::::   */
/*   board.c                                            :+:      :+:    :+:   */
/*                                                    +:+ +:+         +:+     */
/*   By: tgauvrit <tgauvrit@student.42.fr>          +#+  +:+       +#+        */
/*                                                +#+#+#+#+#+   +#+           */
/*   Created: 2016/12/18 16:50:05 by tgauvrit          #+#    #+#             */
/*   Updated: 2017/01/31 11:49:06 by tgauvrit         ###   ########.fr       */
/*                                                                            */
/* ************************************************************************** */

#include "gomoku.h"

int		board_isvalid(int x, int y)
{
	if (x < 0 || x >= 15 || y < 0 || y >= 15)
		return 0;
	return 1;
}

// For line5 loop ONLY
// Use line5_point_all for single point
static uint32_t	line5_iteration(t_board *board, int color, int x, int y)
{
	int		i, j, m, count;

	count = 0;
	for (i = 0; i <= 1; ++i)
		for (j = -1; j <= 1; ++j)
		{
			if (i == 0 && (j == 0 || j == 1))
				continue;
			for (m = 1; m <= 4; ++m)
				if (!board_isvalid(x+i*m, y+j*m) || board->map[x+i*m][y+j*m] != color)
					break;
			if (m == 5)
				++count;
		}
	return count;
}

// Examines full board for line5
void	board_line5(t_board *board, t_player p)
{
	int		x, y;
	t_player o;

	o = player_switch(p);
	board->hrc[0].line5 = board->hrc[1].line5 = 0;
	for (x = 0; x < 15; ++x)
		for (y = 0; y < 15; ++y)
		{
			if (board->map[x][y] == p.color)
				board->hrc[p.maximizing].line5 += line5_iteration(board, p.color, x, y);
			else if (board->map[x][y] == o.color)
				board->hrc[o.maximizing].line5 += line5_iteration(board, o.color, x, y);
		}
}

// Verifies line5 for one point
// Player chip should ALREADY be placed!
void	point_all_line5(t_board *board, t_player p, int x, int y, int mod)
{
	int		i, j, k, m;

	for (i = 0; i <= 1; ++i)
		for (j = -1; j <= 1; ++j)
			if (i == 1 || j == -1)
				for (k = -4; k <= 0; ++k)
				{
					for (m = k; m <= k+4; ++m)
					{
						if (board_isvalid(x+i*m, y+j*m) && board->map[x+i*m][y+j*m] == p.color)
							continue;
						break;
					}
					if (m == k+5)
					{
						board->hrc[p.maximizing].line5 += mod;
						break;
					}
					k = m;
				}
}

// Chip must already be placed
void	point_all_free(t_board *board, t_player p, int x, int y, int mod)
{
	int		i, j, m, n, space;

	for (i = 0; i <= 1; ++i)
		for (j = -1; j <= 1; ++j)
		{
			if (i == 0 && (j == 0 || j == 1))
				continue;
			for (m = -4; m <= 1; ++m)
			{
				if (!board_isvalid(x+i*m, y+j*m) || board->map[x+i*m][y+j*m] != EMPTY)
					continue;
				n = m + 5;
				if (!board_isvalid(x+i*n, y+j*n))
					break;
				if (board->map[x+i*n][y+j*n] != EMPTY)
					continue;
				space = 0;
				for (n -= 1; n > m; --n)
				{
					if (board->map[x+i*n][y+j*n] == EMPTY)
					{
						if (space == 0)
							++space;
						else
							break;
					}
					else if (board->map[x+i*n][y+j*n] != p.color)
						break;
				}
				if (n == m)
				{
					if (space == 0)
						board->hrc[p.maximizing].free4 += mod;
					else
						board->hrc[p.maximizing].free3 += mod;
					break;
				}
			}
		}
}

void	board_free(t_board *board, t_player p)
{
	int		x, y;
	t_player o;

	o = player_switch(p);
	board->hrc[0].free3 = board->hrc[1].free3 = 0;
	board->hrc[0].free4 = board->hrc[1].free4 = 0;
	for (x = 0; x < 15; ++x)
		for (y = 0; y < 15; ++y)
		{
			if (board->map[x][y] == p.color)
				point_all_free(board, p, x, y, 1);
			else if (board->map[x][y] == o.color)
				point_all_free(board, o, x, y, 1);
		}
	// SHOULD PROBABLY DEBUG CHECK THIS AT SOME POINT TO ENSURE IT WORKS AS INTENDED
	// SO FAR THIS WORKS AS INTENDED
	board->hrc[0].free3 = board->hrc[0].free3 / 3;
	board->hrc[1].free3 = board->hrc[1].free3 / 3;
	board->hrc[0].free4 = board->hrc[0].free4 / 4;
	board->hrc[1].free4 = board->hrc[1].free4 / 4;
}

void	point_all(t_board *board, t_player player, int x, int y, int mod)
{
	point_all_line5(board, player, x, y, mod);
	point_all_free(board, player, x, y, mod);
}

// Player chip WILL BE PLACED!
void	board_capture(t_board *board, t_player p, int x, int y)
{
	int 	i, j;
	t_player	o;

	o = player_switch(p);
	board->map[x][y] = EMPTY;
	for (i = -1; i <= 1; ++i)
	{
		for (j = -1; j <= 1; ++j)
		{
			if (i == 0 && j == 0)
				continue;
			if (board_isvalid(x + i  , y + j  ) && board->map[x + i  ][y + j  ] == o.color
			 && board_isvalid(x + i*2, y + j*2) && board->map[x + i*2][y + j*2] == o.color
			 && board_isvalid(x + i*3, y + j*3) && board->map[x + i*3][y + j*3] == p.color)
			{
				point_all(board, o, x + i  , y + j,   -1);
				point_all(board, o, x + i*2, y + j*2, -1);
				board->map[x + i  ][y + j  ] = EMPTY;
				board->map[x + i*2][y + j*2] = EMPTY;
				board->hrc[p.maximizing].captured += 2;
			}
		}
	}
	board->map[x][y] = p.color;
	point_all(board, p, x, y, 1);
}
