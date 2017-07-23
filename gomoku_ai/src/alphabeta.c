/* ************************************************************************** */
/*                                                                            */
/*                                                        :::      ::::::::   */
/*   alphabeta.c                                        :+:      :+:    :+:   */
/*                                                    +:+ +:+         +:+     */
/*   By: tgauvrit <tgauvrit@student.42.fr>          +#+  +:+       +#+        */
/*                                                +#+#+#+#+#+   +#+           */
/*   Created: 2016/12/17 14:22:12 by tgauvrit          #+#    #+#             */
/*   Updated: 2017/01/31 13:06:44 by tgauvrit         ###   ########.fr       */
/*                                                                            */
/* ************************************************************************** */

#include "gomoku.h"

t_ab		ab_new(void)
{
	t_ab	heuristic;

	heuristic.alpha = INT_MIN;
	heuristic.beta  = INT_MAX;
	return heuristic;
}

static int	max(int a, int b)
{
	return (a >= b ? a : b);
}

static int	min(int a, int b)
{
	return (a <= b ? a : b);
}

static int	alphabeta_recurse(t_board *board, t_board *board_memory, int depth, t_ab ab, t_player player)
{
	int		value, i, count;

	if (board->h == INT_MAX || board->h == INT_MIN)
		return board->h;
	if (depth == g_alphabeta_depth)
		return board->h; // Cleaner calculation here maybe?
	count = moves_get_boards(board, player, board_memory);
	if (depth < 3)
		move_boards_sort(board_memory, count);
	if (count == 0)
		return 0;
	if (player.maximizing)
	{
		value = INT_MIN;
		for (i = 0; i < count; ++i)
		{
			value = max(value, alphabeta_recurse(board_memory + i, board_memory + MAX_MOVES, depth + 1, ab, player_switch(player)));
			ab.alpha = max(ab.alpha, value);
			if (ab.beta <= ab.alpha)
				break ; // beta cut-off
		}
		return value;
	}
	else // minimizing
	{
		value = INT_MAX;
		for (i = 0; i < count; ++i)
		{
			value = min(value, alphabeta_recurse(board_memory + i, board_memory + MAX_MOVES, depth + 1, ab, player_switch(player)));
			ab.beta = min(ab.beta, value);
			if (ab.beta <= ab.alpha)
				break ; // alpha cut-off
		}
		return value;
	}
}

// COLOR OF MAXIMIZING PLAYER (AI PLAYER) ALWAYS
t_move	alphabeta(t_board *board, int color)
{
	t_move		*moves;
	t_move		*best;
	int			value, i, count;
	t_ab		ab = ab_new();
	t_player	player = player_create(color, 1);
	t_board		*board_memory;

	moves = malloc(sizeof(t_move) * MAX_MOVES);
	board_memory = malloc(sizeof(t_board) * MAX_MOVES * (g_alphabeta_depth + 1));
	// heuristic_calculate(board);
	count = moves_get(*board, player, moves);
	if (count == 0)
		return move_create(-1, -1);
	best = moves; // By default, first move is best move, jic
	for (i = 0; i < count; ++i)
	{
		board_memory[i] = *board;
		if (heuristic_partial_move(board_memory + i, player, moves[i].x, moves[i].y) == NULL)
			continue;
		value = alphabeta_recurse(board_memory + i, board_memory + MAX_MOVES, 1, ab, player_switch(player));
		if (value > ab.alpha)
		{
			best = moves + i;
			ab.alpha = value;
			if (ab.alpha == INT_MAX)
				break ;
		}
	}
	free(moves);
	free(board_memory);
	return *best;
}
