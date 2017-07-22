/* ************************************************************************** */
/*                                                                            */
/*                                                        :::      ::::::::   */
/*   main.c                                             :+:      :+:    :+:   */
/*                                                    +:+ +:+         +:+     */
/*   By: tgauvrit <tgauvrit@student.42.fr>          +#+  +:+       +#+        */
/*                                                +#+#+#+#+#+   +#+           */
/*   Created: 2016/12/17 11:47:08 by tgauvrit          #+#    #+#             */
/*   Updated: 2017/01/31 13:20:48 by tgauvrit         ###   ########.fr       */
/*                                                                            */
/* ************************************************************************** */

#include "gomoku.h"

/* ************************************************************************** */
#include <sys/time.h>

static struct timeval tm1;

static inline void start()
{
    gettimeofday(&tm1, NULL);
}

static inline void stop()
{
    struct timeval tm2;
    gettimeofday(&tm2, NULL);

    unsigned long long t = 1000 * (tm2.tv_sec - tm1.tv_sec) + (tm2.tv_usec - tm1.tv_usec) / 1000;
    printf("Time: %llu ms\n", t);
}
/* ************************************************************************** */

int		g_alphabeta_depth = 6;

int		main(int argc, char *argv[])
{
	int			fd;
	t_board		*board;
	t_player	player;
	t_move		move;

	if (argc != 2)
	{
		dprintf(2, "Usage: %s NAMED_PIPE\n", argv[0]);
		return 1;
	}
	fd = open(argv[1], O_RDONLY);
	if (fd < 0)
	{
		perror("Input");
		return 1;
	}
	while (42) {
		board = read_board(fd, &player);
		print_board(board, player);
		start();
		move = alphabeta(board, player.color);
		stop();
		dprintf(1, "Move: (%d, %d)\n", move.x, move.y);
	}
	if (fd > 2)
		close(fd);
	return 0;
}
