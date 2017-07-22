/* ************************************************************************** */
/*                                                                            */
/*                                                        :::      ::::::::   */
/*   move.c                                             :+:      :+:    :+:   */
/*                                                    +:+ +:+         +:+     */
/*   By: tgauvrit <tgauvrit@student.42.fr>          +#+  +:+       +#+        */
/*                                                +#+#+#+#+#+   +#+           */
/*   Created: 2016/12/18 12:31:13 by tgauvrit          #+#    #+#             */
/*   Updated: 2017/01/31 12:44:54 by tgauvrit         ###   ########.fr       */
/*                                                                            */
/* ************************************************************************** */

#include "gomoku.h"

int     move_compress(t_move move)
{
	return (move.x << 16 & move.y);
}

t_move  move_decompress(int move)
{
	t_move  new;

	new.x = move >> 16;
	new.y = move & 0xFF;
	return (new);
}

t_move  move_create(t_space x, t_space y)
{
	t_move  new;

	new.x = x;
	new.y = y;
	return new;
}

int		move_max(t_move a, t_move b)
{
	if (a.h > b.h)
		return 1;
	else
		return 0;
}

void	moves_sort_helper(t_move *input, int left, int right, t_move *tmp)
{
	if(right == left + 1)
	{
		return;
	}
	else
	{
		int i = 0;
		int length = right - left;
		int midpoint_distance = length / 2;
		int l = left, r = left + midpoint_distance;
		moves_sort_helper(input, left, left + midpoint_distance, tmp);
		moves_sort_helper(input, left + midpoint_distance, right, tmp);
		for(i = 0; i < length; i++)
		{
			if(l < left + midpoint_distance && (r == right || move_max(input[l], input[r])))
			{
				tmp[i] = input[l];
				l++;
			}
			else
			{
				tmp[i] = input[r];
				r++;
			}
		}
		for(i = left; i < right; i++)
		{
			input[i] = tmp[i - left];
		}
	}
}

void    moves_sort(t_move *moves, int size)
{
	static t_move tmp[MAX_MOVES];
	moves_sort_helper(moves, 0, size, tmp);
	// for (int i = 0; i < size; i++)
	// 	dprintf(1, "Move: (%d, %d) [%d]\n", moves[i].x, moves[i].y, moves[i].h);
}

int		move_board_max(t_board a, t_board b)
{
	if (a.h > b.h)
		return 1;
	else
		return 0;
}

void	move_boards_sort_helper(t_board *input, int left, int right, t_board *tmp)
{
	if(right == left + 1)
	{
		return;
	}
	else
	{
		int i = 0;
		int length = right - left;
		int midpoint_distance = length / 2;
		int l = left, r = left + midpoint_distance;
		move_boards_sort_helper(input, left, left + midpoint_distance, tmp);
		move_boards_sort_helper(input, left + midpoint_distance, right, tmp);
		for(i = 0; i < length; i++)
		{
			if(l < left + midpoint_distance && (r == right || move_board_max(input[l], input[r])))
			{
				tmp[i] = input[l];
				l++;
			}
			else
			{
				tmp[i] = input[r];
				r++;
			}
		}
		for(i = left; i < right; i++)
		{
			input[i] = tmp[i - left];
		}
	}
}

void	move_boards_sort(t_board *move_boards, int size)
{
	static t_board tmp[MAX_MOVES];
	move_boards_sort_helper(move_boards, 0, size, tmp);
	// for (int i = 0; i < size; i++)
	// 	dprintf(1, "Move: (%d, %d) [%d]\n", moves[i].x, moves[i].y, moves[i].h);
}

int		moves_get(t_board board, t_player player, t_move *moves)
{
	int     x, base_x;
	int     y, base_y;
	t_move  *start;
	t_board first = board;
	t_board tmp;

	start = moves;
	for (base_x = 0; base_x < 15; ++base_x)
		for (base_y = 0; base_y < 15; ++base_y)
			if (board.map[base_x][base_y] == WHITE || board.map[base_x][base_y] == BLACK)
				for (x = base_x - 1; x <= base_x + 1; ++x)
					if (x >= 0 && x < 15)
						for (y = base_y - 1; y <= base_y + 1; ++y)
							if (y >= 0 && y < 15)
								if (board.map[x][y] == EMPTY)
								{
									moves->x = x;
									moves->y = y;
									tmp = first;
									if (heuristic_partial_move(&tmp, player, x, y) != NULL)
										moves->h = tmp.h;
									else
										moves->h = 0;
									moves++;
									board.map[x][y] = TEMPT;
								}
	moves_sort(start, moves - start);
	return (moves - start);
}

int		moves_get_boards(t_board *board_model, t_player player, t_board *move_boards)
{
	int     x, base_x;
	int     y, base_y;
	t_board board;
	t_board *start;

	start = move_boards;
	board = *board_model;
	for (base_x = 0; base_x < 15; ++base_x)
		for (base_y = 0; base_y < 15; ++base_y)
			if (board.map[base_x][base_y] == WHITE || board.map[base_x][base_y] == BLACK)
				for (x = base_x - 1; x <= base_x + 1; ++x)
					if (x >= 0 && x < 15)
						for (y = base_y - 1; y <= base_y + 1; ++y)
							if (y >= 0 && y < 15)
								if (board.map[x][y] == EMPTY)
								{
									*move_boards = *board_model;
									if (heuristic_partial_move(move_boards, player, x, y) != NULL)
										move_boards++;
									board.map[x][y] = TEMPT;
								}
	return (move_boards - start);
}
